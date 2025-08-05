using Cashregister.Api.Articles;
using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration.AddEnvironmentVariables("CASHREGISTER_");

builder.Services
  .AddCashregisterDatabase(builder.Configuration)
  .AddCashregisterArticles()
  .AddCashregisterOrders();

var app = builder.Build();

app.MapArticles();

using var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

await dbContext.Database.MigrateAsync();

await app.RunAsync();