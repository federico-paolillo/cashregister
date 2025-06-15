using CashRegister.Application.Orders.Transactions;
using CashRegister.Application.Orders.Transactions.Models.Input;
using CashRegister.Application.Receipts.Services;

using Cashregister.Factories;

namespace Cashregister.UseCases;

public sealed class PlaceOrderActivity(
    ScopeAwareFactory<IPlaceOrderTransaction> placeOrderTx,
    ScopeAwareFactory<IPrintReceiptTransaction> printReceiptTx
)
{
    public async Task PlaceOrderAsync(OrderRequest orderRequest)
    {
        var newOrder = await placeOrderTx.ExecuteAsync(svc => svc.PlaceOrderAsync(orderRequest));

        await printReceiptTx.ExecuteAsync(prnt => prnt.PrintReceiptAsync(newOrder));
    }
}