using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Handlers;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Factories.Problems;

namespace Cashregister.Activities;

public sealed class PlaceOrderActivity(
    Scoped<IPlaceOrderTransaction> placeOrderTx,
    Scoped<IPrintReceiptHandler> printReceiptHandler,
    Scoped<IFetchOrderQuery> fetchOrderQuery
)
{
    public async Task<Result<Order>> PlaceOrderAsync(OrderRequest orderRequest)
    {
        var newOrderResult =
            await placeOrderTx.ExecuteAsync(tx => tx.ExecuteAsync(orderRequest, CancellationToken.None));

        if (newOrderResult.NotOk)
        {
            return Result.Error<Order>(newOrderResult.Error);
        }

        var newOrderId = newOrderResult.Value;

        var printReceiptResult =
            await printReceiptHandler.ExecuteAsync(handler => handler.ExecuteAsync(newOrderId));

        if (printReceiptResult.NotOk)
        {
            return Result.Error<Order>(printReceiptResult.Error);
        }

        var order = await fetchOrderQuery.ExecuteAsync(tx => tx.FetchAsync(newOrderId));

        if (order is null)
        {
            return Result.Error<Order>(new BrokenRealityProblem(
                $"We saved the order and printed the receipt but somehow we could not retrieve it. Order is {newOrderId.Value}"));
        }

        return Result.Ok(order);
    }
}