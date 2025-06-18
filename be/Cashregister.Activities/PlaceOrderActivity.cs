using Cashregister.Activities.Exceptions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Queries;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Services;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Activities;

public sealed class PlaceOrderActivity(
    Scoped<IPlaceOrderTransaction> placeOrderTx,
    Scoped<IPrintReceiptTransaction> printReceiptTx,
    Scoped<IFetchOrderSummaryQuery> fetchOrderSummaryQuery
)
{
    public async Task<OrderSummary> PlaceOrderAsync(OrderRequest orderRequest)
    {
        Identifier newOrderId = await placeOrderTx.ExecuteAsync(svc => svc.PlaceOrderAsync(orderRequest));

        await printReceiptTx.ExecuteAsync(prnt => prnt.PrintReceiptAsync(newOrderId));

        OrderSummary? orderSummary = await fetchOrderSummaryQuery.ExecuteAsync(tx => tx.FetchAsync(newOrderId));

        if (orderSummary is null)
        {
            throw new BrokenRealityException(
                "We placed the order, then printed it but we were not able to retrieve it");
        }

        return orderSummary;
    }
}