using ImportManager;
using ImportManager.Data;
using ImportManager.Endpoints;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

builder.Services.AddMySQLServer<ImportManagerContext>(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty);
builder.Services.AddHostedService<ImportCustomersBackgroundService>();

var app = builder.Build();

#region Create db

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<ImportManagerContext>();
await context.Database.EnsureCreatedAsync();

#endregion

app.MapImportCustomers();
app.MapImportCustomersStatus();

app.Run();