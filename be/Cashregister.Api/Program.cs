using Cashregister.Api.Articles;
using Cashregister.Api.Orders;
using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(
  options =>
  {
      options.SingleLine = true;
      options.TimestampFormat = "HH:mm:ss ";
      options.UseUtcTimestamp = true;
      options.ColorBehavior = LoggerColorBehavior.Disabled;
      options.IncludeScopes = true;
  }
);

builder.Configuration.AddEnvironmentVariables("CASHREGISTER_");

builder.Services
  .AddCashregisterDatabase(builder.Configuration)
  .AddCashregisterArticles()
  .AddCashregisterOrders();

var app = builder.Build();

app.MapArticles();
app.MapOrders();

using var scope = app.Services.CreateScope();

using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

await dbContext.Database.MigrateAsync();

await app.RunAsync();