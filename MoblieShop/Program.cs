using MoblieShop.Data;
using MoblieShop.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureApplication();

var app = builder.Build();
app.ConfigureApplicationPipeline();

await app.ApplyDatabaseMigrationsAsync();
await SeedData.Initialize(app.Services);
app.MapApplicationEndpoints();
app.Run();
