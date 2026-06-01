using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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