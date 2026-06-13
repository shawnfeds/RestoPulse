using MassTransit;
using RestoPulse.TableService.Api.Endpoints;
using RestoPulse.TableService.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<TableDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("tabledb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var messagingConnectionString = builder.Configuration.GetConnectionString("messaging");
if (!string.IsNullOrEmpty(messagingConnectionString))
{
    builder.Services.AddMassTransit(x =>
    {
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
        var db = scope.ServiceProvider.GetRequiredService<TableDbContext>();
        await db.Database.MigrateAsync();
        await TableDbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration failed. Service will continue and retry on subsequent database operations.");
    }
}

var tables = app.MapGroup("/api/tables").WithTags("Tables");
tables.MapTableEndpoints();

app.Run();