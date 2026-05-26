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

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BillSettledConsumer>();
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));

        cfg.ReceiveEndpoint("report-bill-settled", e =>
            e.ConfigureConsumer<BillSettledConsumer>(ctx));

        cfg.ReceiveEndpoint("report-order-created", e =>
            e.ConfigureConsumer<OrderCreatedConsumer>(ctx));
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    await db.Database.MigrateAsync();
}

var reports = app.MapGroup("/api/reports").WithTags("Reports");
reports.MapReportEndpoints();

app.Run();