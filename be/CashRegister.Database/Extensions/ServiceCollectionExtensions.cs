using Cashregister.Application;
using Cashregister.Application.Orders.Commands;
using Cashregister.Application.Orders.Queries;
using Cashregister.Database.Commands;
using Cashregister.Database.Interceptors;
using Cashregister.Database.Mappers;
using Cashregister.Database.Queries;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Database.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AddDbContext(services, configuration);
        AddMappers(services);
        AddQueriesAndCommands(services);

        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        string? dataSource = configuration.GetValue<string>("DataSource");

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new InvalidOperationException("Configuration setting 'DataSource' is required.");
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = dataSource,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = true
        };

        string connectionString = connectionStringBuilder.ToString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The database connection string 'CashRegister' is missing.");
        }

        services.AddScoped<SqlitePragmasDbConnectionInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, opts) =>
            opts.UseSqlite(connectionString)
                .AddInterceptors(sp.GetRequiredService<SqlitePragmasDbConnectionInterceptor>())
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
        );

        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationDbContext>(x => x.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddQueriesAndCommands(IServiceCollection services)
    {
        services.AddScoped<ISaveOrderCommand, SaveOrderCommand>();
        services.AddScoped<IFetchArticlesQuery, FetchArticlesQuery>();
        services.AddScoped<IFetchOrderSummaryQuery, FetchOrderSummaryQuery>();
    }

    private static void AddMappers(IServiceCollection services)
    {
        services.AddSingleton<OrderEntityMapper>();
        services.AddSingleton<ArticleEntityMapper>();
    }
}