using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Api.Endpoints;
using RestoPulse.InventoryService.Infrastructure.Messaging;
using RestoPulse.InventoryService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("inventorydb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var messagingConnectionString = builder.Configuration.GetConnectionString("messaging");
if (!string.IsNullOrEmpty(messagingConnectionString))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<OrderStatusChangedConsumer>();

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
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration failed. Service will continue and retry on subsequent database operations.");
    }
}

var inventory = app.MapGroup("/api/inventory").WithTags("Inventory");
inventory.MapInventoryEndpoints();

app.Run();