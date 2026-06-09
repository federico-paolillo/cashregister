using Cashregister.Application.Receipts.Handlers;
using Cashregister.Application.Receipts.Handlers.Defaults;
using Cashregister.Application.Receipts.Services;
using Cashregister.Application.Receipts.Services.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Receipts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterReceipts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RomeTimeConverter>();
        serviceCollection.AddScoped<IReceiptPrintProgramService, ReceiptPrintProgramService>();
        serviceCollection.AddScoped<IPrintReceiptHandler, PrintReceiptHandler>();

        return serviceCollection;
    }
}