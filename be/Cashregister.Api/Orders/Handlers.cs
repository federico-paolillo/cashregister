using System.Collections.Immutable;

using Cashregister.Api.Orders.Models;
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

    return TypedResults.Created($"/orders/{orderResult.Value}", orderRequestDto);
  }
}