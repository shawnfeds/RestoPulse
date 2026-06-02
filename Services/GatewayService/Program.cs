using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("sliding-policy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Get matched YARP route ID if available to partition per client IP per API route
        var endpoint = httpContext.GetEndpoint();
        var routeId = endpoint?.Metadata.GetMetadata<Yarp.ReverseProxy.Model.RouteModel>()?.Config.RouteId 
                      ?? httpContext.Request.Path.Value 
                      ?? "default";

        var partitionKey = $"{clientIp}:{routeId}";

        var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var rateLimitConfig = config.GetSection("RateLimiting");

        var permitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 60);
        var windowSeconds = rateLimitConfig.GetValue<int>("WindowSeconds", 60);
        var segmentsPerWindow = rateLimitConfig.GetValue<int>("SegmentsPerWindow", 6);
        var queueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 1000);

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                SegmentsPerWindow = segmentsPerWindow,
                QueueLimit = queueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});

// ── YARP ──────────────────────────────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

// ── JWT Bearer (optional in development) ───────────────────────────────────────
var jwtAuthority = builder.Configuration["Jwt:Authority"];
var jwtAudience  = builder.Configuration["Jwt:Audience"];
var jwtConfigured = !string.IsNullOrWhiteSpace(jwtAuthority) &&
                    !jwtAuthority.Contains("your-identity-provider", StringComparison.OrdinalIgnoreCase);

if (jwtConfigured)
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.Audience = jwtAudience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT auth failed: {Error}", ctx.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });
}

builder.Services.AddAuthorization(opts =>
{
    // Always define RestroAuth policy - either strict or permissive based on config
    if (jwtConfigured)
        opts.AddPolicy("RestroAuth", policy => policy.RequireAuthenticatedUser());
    else
        // In dev without real auth provider, allow all requests
        opts.AddPolicy("RestroAuth", policy => policy.RequireAssertion(_ => true));
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Health checks (via Aspire defaults) ───────────────────────────────────────
var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors();

// Always use authorization (RestroAuth policy is always defined above)
if (jwtConfigured)
{
    app.UseAuthentication();
}
app.UseAuthorization();

app.UseRateLimiter();

app.UseDefaultFiles();
app.UseStaticFiles();

// YARP middleware — must come after auth
app.MapReverseProxy(pipeline =>
{
    pipeline.UseSessionAffinity();
    pipeline.UseLoadBalancing();
    pipeline.UsePassiveHealthChecks();
});

app.Run();