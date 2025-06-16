using CashRegister.Application.Receipts.Queries;
using CashRegister.Domain;

namespace CashRegister.Application.Receipts.Services.Defaults;

public sealed class PrintReceiptTransaction(
    IFetchOrderPrintDataQuery fetchOrderPrintDataQuery
) : IPrintReceiptTransaction
{
    public async Task PrintReceiptAsync(Identifier orderId)
    {
        var printData = await fetchOrderPrintDataQuery.Fetch(orderId);
    }
}