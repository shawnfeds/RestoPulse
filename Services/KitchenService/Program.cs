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

builder.Services.AddMassTransit(x =>
{
    // Register consumer — this creates the queue automatically
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.ConfigureEndpoints(ctx);
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
    var db = scope.ServiceProvider.GetRequiredService<KitchenDbContext>();
    await db.Database.MigrateAsync();
}

var kitchen = app.MapGroup("/api/kitchen").WithTags("Kitchen");
kitchen.MapKitchenEndpoints();

app.Run();