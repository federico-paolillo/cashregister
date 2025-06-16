using CashRegister.Application.Orders.Transactions;
using CashRegister.Application.Orders.Transactions.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace CashRegister.Application.Orders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterOrders(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPlaceOrderTransaction, PlaceOrderTransaction>();
        
        return serviceCollection;
    }
}