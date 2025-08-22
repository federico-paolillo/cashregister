using System.Globalization;

using Cashregister.Application.Articles.Extensions;
using Cashregister.Application.Orders.Extensions;
using Cashregister.Application.Receipts.Extensions;
using Cashregister.Database;
using Cashregister.Database.Extensions;
using Cashregister.Factories;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration;

public abstract class IntegrationTest(
    ITestOutputHelper testOutputHelper
) : IDisposable
{
    private string? _dataSource;

    private WebApplicationFactory<Program>? _webApplicationFactory;

    public void Dispose()
    {
        _webApplicationFactory?.Dispose();

        if (_dataSource is not null)
        {
            File.Delete(Path.Combine(".", _dataSource));
        }
    }

    protected async Task PrepareEnvironmentAsync()
    {
        _dataSource = GenerateDatabaseName();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataSource"] = _dataSource
            })
            .Build();

        _webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(l => l.AddProvider(new XUnitLoggerProvider(testOutputHelper)));
                builder.UseEnvironment(Environments.Development);
                builder.UseConfiguration(cfg);
            });

        await ApplyMigrationsAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        if (_webApplicationFactory is null)
        {
            throw new InvalidOperationException("No WebApplicationFactory Did you call PrepareEnvironmentAsync()?");
        }

        using IServiceScope scope = NewServiceScope();

        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    protected IServiceScope NewServiceScope()
    {
        if (_webApplicationFactory is null)
        {
            throw new InvalidOperationException("No WebApplicationFactory Did you call PrepareEnvironmentAsync()?");
        }

        return _webApplicationFactory.Services.CreateScope();
    }

    protected Task<TResult> RunScoped<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        using var serviceScope = NewServiceScope();

        var service = serviceScope.ServiceProvider.GetRequiredService<TService>();

        return action(service);
    }

    protected HttpClient CreateHttpClient()
    {
        if (_webApplicationFactory is null)
        {
            throw new InvalidOperationException("No WebApplicationFactory Did you call PrepareEnvironmentAsync()?");
        }

        return _webApplicationFactory.CreateClient();
    }

    private static string GenerateDatabaseName()
    {
        string now = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return $"{Ulid.NewUlid()}-{now}.sqlite3";
    }
}