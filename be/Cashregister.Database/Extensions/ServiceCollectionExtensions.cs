using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Database.Commands;
using Cashregister.Database.Interceptors;
using Cashregister.Database.Mappers;
using Cashregister.Database.Queries;
using Cashregister.Factories;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Database.Extensions;

public static class ServiceCollectionExtensions
{
    private const string ConnectionStringName = "DataSource";

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
        var dataSource = configuration.GetValue<string>(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new InvalidOperationException($"Configuration setting '{ConnectionStringName}' is required.");
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = dataSource,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = true
        };

        var connectionString = connectionStringBuilder.ToString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"The database connection string '{ConnectionStringName}' is missing.");
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
        services.AddScoped<IFetchOrderQuery, FetchOrderQuery>();
        services.AddScoped<IFetchArticleQuery, FetchArticleQuery>();
        services.AddScoped<ISaveArticleCommand, SaveArticleCommand>();
        services.AddScoped<IPaginationQuery<ArticleListItem>, FetchArticlesListQuery>();
        services.AddScoped<IPaginationQuery<OrderListItem>, FetchOrdersListQuery>();
    }

    private static void AddMappers(IServiceCollection services)
    {
        services.AddSingleton<OrderEntityMapper>();
        services.AddSingleton<ArticleEntityMapper>();
    }
}