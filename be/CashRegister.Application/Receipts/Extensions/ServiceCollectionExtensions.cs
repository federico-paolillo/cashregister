using Cashregister.Application.Receipts.Services;
using Cashregister.Application.Receipts.Services.Defaults;

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