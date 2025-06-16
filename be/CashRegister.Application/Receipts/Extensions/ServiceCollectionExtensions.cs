using CashRegister.Application.Orders.Transactions;
using CashRegister.Application.Orders.Transactions.Defaults;
using CashRegister.Application.Receipts.Services;
using CashRegister.Application.Receipts.Services.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace CashRegister.Application.Receipts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterReceipts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPrintReceiptTransaction, PrintReceiptTransaction>();
        
        return serviceCollection;
    }
}