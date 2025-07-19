using Cashregister.Activities.Exceptions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Queries;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Transactions;
using Cashregister.Factories;

namespace Cashregister.Activities;

public sealed class PlaceOrderActivity(
    Scoped<IPlaceOrderTransaction> placeOrderTx,
    Scoped<IPrintReceiptTransaction> printReceiptTx,
    Scoped<IFetchOrderSummaryQuery> fetchOrderSummaryQuery
)
{
    public async Task<Result<OrderSummary>> PlaceOrderAsync(OrderRequest orderRequest)
    {
        var newOrderResult =
            await placeOrderTx.ExecuteAsync(tx => tx.ExecuteAsync(orderRequest, CancellationToken.None));

        if (newOrderResult.NotOk)
        {
            return Result.Error<OrderSummary>(newOrderResult.Error);
        }

        var newOrderId = newOrderResult.Value;

        var printReceiptResult =
            await printReceiptTx.ExecuteAsync(tx => tx.ExecuteAsync(newOrderId, CancellationToken.None));

        if (printReceiptResult.NotOk)
        {
            return Result.Error<OrderSummary>(printReceiptResult.Error);
        }

        var orderSummary = await fetchOrderSummaryQuery.ExecuteAsync(tx => tx.FetchAsync(newOrderId));

        if (orderSummary is null)
        {
            throw new BrokenRealityException(
                "We placed the order then printed it but we were not able to retrieve it from the database");
        }

        return Result.Ok(orderSummary);
    }
}