using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.KitchenService.Api.Endpoints;
using RestoPulse.KitchenService.Infrastructure.Messaging;
using RestoPulse.KitchenService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<KitchenDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("kitchendb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var messagingConnectionString = builder.Configuration.GetConnectionString("messaging");
if (!string.IsNullOrEmpty(messagingConnectionString))
{
    builder.Services.AddMassTransit(x =>
    {
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
        var db = scope.ServiceProvider.GetRequiredService<KitchenDbContext>();
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration failed. Service will continue and retry on subsequent database operations.");
    }
}

var kitchen = app.MapGroup("/api/kitchen").WithTags("Kitchen");
kitchen.MapKitchenEndpoints();

app.Run();