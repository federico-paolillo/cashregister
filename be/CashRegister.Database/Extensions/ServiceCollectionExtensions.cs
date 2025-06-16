using CashRegister.Application;
using CashRegister.Application.Orders.Commands;
using CashRegister.Application.Orders.Queries;
using CashRegister.Database.Commands;
using CashRegister.Database.Interceptors;
using CashRegister.Database.Mappers;
using CashRegister.Database.Queries;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashRegister.Database.Extensions;

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
        var dataSource = configuration.GetValue<string>("DataSource");

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new InvalidOperationException("Configuration setting 'DataSource' is required."); 
        }

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = dataSource,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = true
        };
        
        var connectionString = connectionStringBuilder.ToString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The database connection string 'CashRegister' is missing.");
        }

        services.AddScoped<SqlitePragmasDbConnectionInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp , opts)=>
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