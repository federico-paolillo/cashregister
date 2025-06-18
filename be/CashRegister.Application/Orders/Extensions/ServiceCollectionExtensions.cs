using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Orders.Transactions.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Orders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterOrders(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlaceOrderTransaction, PlaceOrderTransaction>();

        return serviceCollection;
    }
}