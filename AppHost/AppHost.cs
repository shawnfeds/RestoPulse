var builder = DistributedApplication.CreateBuilder(args);

// ── Infrastructure ─────────────────────────────────────────
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var rabbit = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

// ── Databases ──────────────────────────────────────────────
var menuDb = sqlServer.AddDatabase("menudb");
var tableDb = sqlServer.AddDatabase("tabledb");
var orderDb = sqlServer.AddDatabase("orderdb");
var kitchenDb = sqlServer.AddDatabase("kitchendb");
var billingDb = sqlServer.AddDatabase("billingdb");
var inventoryDb = sqlServer.AddDatabase("inventorydb");
var reportDb = sqlServer.AddDatabase("reportdb");
var userDb = sqlServer.AddDatabase("userdb");
var notificationDb = sqlServer.AddDatabase("notificationdb");

// ── Services ───────────────────────────────────────────────
var menuSvc = builder.AddProject<Projects.RestoPulse_MenuService>("menu-service")
    .WithReference(menuDb)
    .WaitFor(menuDb);

var tableSvc = builder.AddProject<Projects.RestoPulse_TableService>("table-service")
    .WithReference(tableDb)
    .WithReference(rabbit)
    .WaitFor(tableDb)
    .WaitFor(rabbit);

var orderSvc = builder.AddProject<Projects.RestoPulse_OrderService>("order-service")
    .WithReference(orderDb)
    .WithReference(rabbit)
    .WaitFor(orderDb)
    .WaitFor(rabbit);

var kitchenSvc = builder.AddProject<Projects.RestoPulse_KitchenService>("kitchen-service")
    .WithReference(kitchenDb)
    .WithReference(rabbit)
    .WaitFor(kitchenDb)
    .WaitFor(rabbit);

var billingSvc = builder.AddProject<Projects.RestoPulse_BillingService>("billing-service")
    .WithReference(billingDb)
    .WithReference(rabbit)
    .WaitFor(billingDb)
    .WaitFor(rabbit);

var inventorySvc = builder.AddProject<Projects.RestoPulse_InventoryService>("inventory-service")
    .WithReference(inventoryDb)
    .WithReference(rabbit)
    .WaitFor(inventoryDb)
    .WaitFor(rabbit);

var reportSvc = builder.AddProject<Projects.RestoPulse_ReportService>("report-service")
    .WithReference(reportDb)
    .WithReference(rabbit)
    .WaitFor(reportDb)
    .WaitFor(rabbit);

var userSvc = builder.AddProject<Projects.RestoPulse_UserService>("user-service")
    .WithReference(userDb)
    .WaitFor(userDb);

var notificationSvc = builder.AddProject<Projects.RestoPulse_NotificationService>("notification-service")
    .WithReference(notificationDb)
    .WithReference(rabbit)
    .WaitFor(notificationDb)
    .WaitFor(rabbit);

// ── Gateway (all frontend traffic enters here) ─────────────
builder.AddProject<Projects.RestoPulse_GatewayService>("gateway-service")
    .WithReference(menuSvc)
    .WithReference(tableSvc)
    .WithReference(orderSvc)
    .WithReference(kitchenSvc)
    .WithReference(billingSvc)
    .WithReference(inventorySvc)
    .WithReference(reportSvc)
    .WithReference(userSvc)
    .WithReference(notificationSvc)
    .WithExternalHttpEndpoints();

builder.Build().Run();