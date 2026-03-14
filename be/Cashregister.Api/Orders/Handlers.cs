using System.Collections.Immutable;

using Cashregister.Api.Commons.Models;
using Cashregister.Api.Orders.Models;
using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Handlers;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Pagination;
using Cashregister.Domain;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.Orders;

internal static class Handlers
{
    public static async Task<Results<BadRequest, Ok<OrdersPageDto>>> GetOrdersPage(
        IFetchOrdersPageHandler fetchOrdersPageHandler,
        [FromQuery(Name = "pageSize")] uint pageSize = 50,
        [FromQuery(Name = "after")] string? after = null,
        [FromQuery(Name = "until")] string? until = null
    )
    {
        if (after is not null && until is not null)
        {
            return TypedResults.BadRequest();
        }

        var afterIdentifier = after is not null ? Identifier.From(after) : null;
        var untilIdentifier = until is not null ? Identifier.From(until) : null;

        var pageRequest = new PageRequest
        {
            After = afterIdentifier,
            Until = untilIdentifier,
            Size = pageSize,
        };

        var ordersPageResult = await fetchOrdersPageHandler.ExecuteAsync(pageRequest);

        if (ordersPageResult.NotOk)
        {
            return TypedResults.BadRequest();
        }

        var ordersPage = ordersPageResult.Value;

        var orderListItemDtos = ordersPage.Items
            .Select(OrderListItemDto.From)
            .ToImmutableArray();

        var ordersPageDto = new OrdersPageDto(
            ordersPage.Next?.Value,
            ordersPage.HasNext,
            orderListItemDtos
        );

        return TypedResults.Ok(ordersPageDto);
    }

    public static async Task<Results<BadRequest, Created<EntityPointerDto>>> CreateOrder(
        IPlaceOrderTransaction placeOrderTransaction,
        LinkGenerator linkGenerator,
        [FromBody] OrderRequestDto orderRequestDto
    )
    {
        ImmutableArray<OrderRequestItem> orderItems =
        [
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
            new
            {
                id = orderResult.Value.Value
            }
        );

        if (getOrderUrl is null)
        {
            throw new InvalidOperationException("Failed to generate location for order");
        }

        var orderPointerDto = new EntityPointerDto
        {
            Id = orderResult.Value.Value,
            Location = getOrderUrl
        };

        return TypedResults.Created(getOrderUrl, orderPointerDto);
    }

    public static async Task<Results<NotFound, Ok<OrderDto>>> GetOrder(
        IFetchOrderQuery fetchOrderQuery,
        [FromRoute] string id
    )
    {
        var identifier = Identifier.From(id);

        var order = await fetchOrderQuery.FetchAsync(identifier);

        if (order is null)
        {
            return TypedResults.NotFound();
        }

        var orderDto = OrderDto.From(order);

        return TypedResults.Ok(orderDto);
    }
}