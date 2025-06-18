using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Application.Receipts.Queries;
using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Services.Defaults;

public sealed class PrintReceiptTransaction(
    IFetchOrderPrintDataQuery fetchOrderPrintDataQuery
) : IPrintReceiptTransaction
{
    public async Task PrintReceiptAsync(Identifier orderId)
    {
        OrderPrintData? printData = await fetchOrderPrintDataQuery.Fetch(orderId);
    }
}