using System.Collections.Immutable;

using Cashregister.Api.Orders.Models;
using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.Orders;

internal static class Handlers
{
    public static async Task<Results<BadRequest, Created<OrderRequestDto>>> CreateOrder(
      IPlaceOrderTransaction placeOrderTransaction,
      LinkGenerator linkGenerator,
      [FromBody] OrderRequestDto orderRequestDto
    )
    {
        ImmutableArray<OrderRequestItem> orderItems = [
          .. orderRequestDto.Items.Select(item =>
      new OrderRequestItem
      {
        Article = Identifier.From(item.Article),
        Quantity = item.Quantity
      })
        ];

        var orderRequest = new OrderRequest
        {
            Items = orderItems
        };

        var orderResult = await placeOrderTransaction.ExecuteAsync(orderRequest);

        if (orderResult.NotOk)
        {
            return TypedResults.BadRequest();
        }

        var getOrderUrl = linkGenerator.GetPathByName(
          "GetOrder",
          new { id = orderResult.Value.Value }
        );

        return TypedResults.Created(getOrderUrl, orderRequestDto);
    }

    public static async Task<Results<NotFound, Ok<OrderSummaryDto>>> GetOrder(
      IFetchOrderSummaryQuery fetchOrderSummaryQuery,
      [FromRoute] string id
      )
    {
        var identifier = Identifier.From(id);

        var orderSummary = await fetchOrderSummaryQuery.FetchAsync(identifier);

        if (orderSummary is null)
        {
            return TypedResults.NotFound();
        }

        var orderSummaryDto = OrderSummaryDto.From(orderSummary);

        return TypedResults.Ok(orderSummaryDto);
    }
}