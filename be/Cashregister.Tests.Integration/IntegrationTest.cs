using System.Globalization;

using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Application.Receipts.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;
using Cashregister.Factories;

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
    private string? _dataSource;
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        if (_dataSource is not null)
        {
            File.Delete(Path.Combine(".", _dataSource));
        }

        _serviceProvider?.Dispose();
    }

    protected async Task PrepareEnvironmentAsync()
    {
        _dataSource = GenerateDatabaseName();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataSource"] = _dataSource
            })
            .Build();

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddLogging(bld => bld.AddProvider(new XUnitLoggerProvider(testOutputHelper)))
            .AddCashregisterDatabase(configuration)
            .AddCashregisterOrders()
            .AddCashregisterReceipts()
            .AddCashregisterArticles();

        serviceCollection.AddTransient(typeof(Scoped<>), typeof(Scoped<>));

        _serviceProvider = serviceCollection.BuildServiceProvider();

        await ApplyMigrationsAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("No service provider. Did you call PrepareEnvironmentAsync()?");
        }

        using IServiceScope scope = _serviceProvider.CreateScope();

        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

    protected Task<TResult> RunScoped<TService, TResult>(Func<TService, Task<TResult>> action) 
        where TService : notnull
    {
        using var serviceScope = NewServiceScope();
        
        var service = serviceScope.ServiceProvider.GetRequiredService<TService>();
        
        return action(service);
    }

    private static string GenerateDatabaseName()
    {
        string now = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return $"{Ulid.NewUlid()}-{now}.sqlite3";
    }
}