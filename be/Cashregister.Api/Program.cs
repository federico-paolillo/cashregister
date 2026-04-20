using Cashregister.Activities.Extensions;
using Cashregister.Api.Articles;
using Cashregister.Api.Devices;
using Cashregister.Api.Orders;
using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Application.Receipts.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Encoders;

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

builder.Services.Configure<FileDeviceSettings>(builder.Configuration.GetSection(FileDeviceSettings.Section));

builder.Services
    .AddCashregisterDatabase(builder.Configuration)
    .AddCashregisterArticles()
    .AddCashregisterOrders()
    .AddCashregisterReceipts()
    .AddCashregisterActivities();

builder.Services.AddSingleton<FileDeviceTargetStore>();
builder.Services.AddScoped<IPrinterDeviceCatalog, FilePrinterDeviceCatalog>();
builder.Services.AddScoped<FileDeviceTargetSelector>();
builder.Services.AddScoped<IDevice, FileDevice>();
builder.Services.AddScoped<IEncoder<byte[]>, BinaryEncoder>();

var app = builder.Build();

app.MapArticles();
app.MapOrders();
app.MapDevices();

await ApplyMigrationsAsync(app);

await app.RunAsync();

return;

static async Task ApplyMigrationsAsync(WebApplication webApplication)
{
    // A local function ensures that the scope and the DbContext for migrations gets released before continuing

    using var scope = webApplication.Services.CreateScope();

    await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}