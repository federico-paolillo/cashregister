using CashRegister.Application.Orders.Extensions;
using CashRegister.Application.Receipts.Extensions;
using CashRegister.Database;
using CashRegister.Database.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration;

public abstract class IntegrationTest(
    ITestOutputHelper testOutputHelper
) : IDisposable
{
    private ServiceProvider? _serviceProvider;
    
    private string? _dataSource;

    protected async Task PrepareEnvironmentAsync()
    {
        _dataSource = GenerateDatabaseName();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DataSource"] = _dataSource })
            .Build();

        var serviceCollection = new ServiceCollection()
            .AddLogging(bld => bld.AddProvider(new XUnitLoggerProvider(testOutputHelper)))
            .AddCashregisterDatabase(configuration)
            .AddCashregisterOrders()
            .AddCashregisterReceipts();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        await ApplyMigrationsAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("No service provider. Did you call PrepareEnvironmentAsync()?");
        }
        
        using var scope = _serviceProvider.CreateScope();

        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    protected IServiceScope NewServiceScope()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("No service provider. Did you call PrepareEnvironmentAsync()?");
        }
        
        return _serviceProvider.CreateScope();
    }

    private static string GenerateDatabaseName()
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

        return $"{Ulid.NewUlid()}-{now}.sqlite3";
    }

    public void Dispose()
    {
        if (_dataSource is not null)
        {
            File.Delete(Path.Combine(".", _dataSource));
        }

        _serviceProvider?.Dispose();
    }
}