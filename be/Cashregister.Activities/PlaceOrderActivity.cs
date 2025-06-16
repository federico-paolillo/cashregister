using CashRegister.Application.Orders.Models.Input;
using CashRegister.Application.Orders.Models.Output;
using CashRegister.Application.Orders.Queries;
using CashRegister.Application.Orders.Transactions;
using CashRegister.Application.Receipts.Services;

using Cashregister.Factories;
using Cashregister.UseCases.Exceptions;

namespace Cashregister.UseCases;

public sealed class PlaceOrderActivity(
    Scoped<IPlaceOrderTransaction> placeOrderTx,
    Scoped<IPrintReceiptTransaction> printReceiptTx,
    Scoped<IFetchOrderSummaryQuery> fetchOrderSummaryQuery
)
{
    public async Task<OrderSummary> PlaceOrderAsync(OrderRequest orderRequest)
    {
        var newOrderId = await placeOrderTx.ExecuteAsync(svc => svc.PlaceOrderAsync(orderRequest));

        await printReceiptTx.ExecuteAsync(prnt => prnt.PrintReceiptAsync(newOrderId));

        var orderSummary = await fetchOrderSummaryQuery.ExecuteAsync(tx => tx.FetchAsync(newOrderId));

        if (orderSummary is null)
        {
            throw new BrokenRealityException("We placed the order, thne printed it but we were not able to retrieve it");
        }

        return orderSummary;
    }
}