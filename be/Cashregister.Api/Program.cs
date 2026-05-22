using Cashregister.Activities.Extensions;
using Cashregister.Api.Articles;
using Cashregister.Api.Devices;
using Cashregister.Api.Orders;
using Cashregister.Api.Statistics;
using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Devices.Extensions;
using Cashregister.Application.Devices.Services;
using Cashregister.Application.Devices.Services.Defaults;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Application.Receipts.Extensions;
using Cashregister.Application.Statistics.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
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
    .AddCashregisterOrders()
    .AddCashregisterReceipts()
    .AddCashregisterStatistics()
    .AddCashregisterDevices()
    .AddCashregisterActivities();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMarkdownDevice(builder.Configuration);
}
else
{
    builder.Services.AddFileDevice();
}

var app = builder.Build();

app.MapArticles();
app.MapOrders();
app.MapDevices();
app.MapStatistics();

await ApplyMigrationsAsync(app);
await PreselectPrinterAsync(app);

await app.RunAsync();

return;

static async Task ApplyMigrationsAsync(WebApplication webApplication)
{
    // A local function ensures that the scope and the DbContext for migrations gets released before continuing

    using var scope = webApplication.Services.CreateScope();

    await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

static async Task PreselectPrinterAsync(WebApplication webApplication)
{
    // A local function ensures that the scope gets released before continuing

    using var scope = webApplication.Services.CreateScope();

    var catalog = scope.ServiceProvider.GetRequiredService<IPrinterDeviceCatalog>();
    var selector = scope.ServiceProvider.GetRequiredService<FileDeviceTargetSelector>();

    var devices = await catalog.ListAsync(CancellationToken.None);

    if (devices.Count > 0)
    {
        await selector.SelectAsync(devices[0].Id, CancellationToken.None);
    }
}