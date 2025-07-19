using Cashregister.Application.Receipts.Transactions;
using Cashregister.Application.Receipts.Transactions.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Receipts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterReceipts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPrintReceiptTransaction, PrintReceiptTransaction>();

        return serviceCollection;
    }
}