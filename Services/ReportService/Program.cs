using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Api.Endpoints;
using RestoPulse.ReportService.Infrastructure.Messaging;
using RestoPulse.ReportService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// EF Core — Aspire injects "reportdb" connection string
builder.Services.AddDbContext<ReportDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("reportdb")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// HttpClient for service-to-service calls
builder.Services.AddHttpClient();

// MassTransit + RabbitMQ
var messagingConnectionString = builder.Configuration.GetConnectionString("messaging");
if (!string.IsNullOrEmpty(messagingConnectionString))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<BillSettledConsumer>();
        x.AddConsumer<OrderCreatedConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(messagingConnectionString);
            cfg.ConfigureEndpoints(ctx);
        });
    });
}

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
        await db.Database.MigrateAsync();
        await ReportDbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration failed. Service will continue and retry on subsequent database operations.");
    }
}

var reports = app.MapGroup("/api/reports").WithTags("Reports");
reports.MapReportEndpoints();

var dashboards = app.MapGroup("/api/dashboard/").WithTags("Dashboards");
dashboards.MapDashboardEndpoints();

app.Run();