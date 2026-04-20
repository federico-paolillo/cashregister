using Cashregister.Factories;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Activities.Extensions;

/// <summary>
///     Registers activity orchestration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterActivities(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(typeof(Scoped<>));
        serviceCollection.AddScoped<PlaceOrderActivity>();

        return serviceCollection;
    }
}