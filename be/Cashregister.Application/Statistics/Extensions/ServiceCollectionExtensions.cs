using Cashregister.Application.Statistics.Handlers;
using Cashregister.Application.Statistics.Handlers.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Statistics.Extensions;

/// <summary>
/// Registers statistics application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterStatistics(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWriteSalesStatisticsCsvHandler, WriteSalesStatisticsCsvHandler>();

        return serviceCollection;
    }
}