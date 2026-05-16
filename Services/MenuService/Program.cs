using Microsoft.EntityFrameworkCore;
using RestoPulse.MenuService.Api.Endpoints;
using RestoPulse.MenuService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<MenuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("menudb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
    await db.Database.MigrateAsync();
}

var menu = app.MapGroup("/api/menu").WithTags("Menu");
menu.MapGroup("/categories").MapCategoryEndpoints();
menu.MapGroup("/items").MapMenuItemEndpoints();

app.Run();