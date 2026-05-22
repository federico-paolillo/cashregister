using System.Collections.Immutable;

using Cashregister.Application.Orders.Models.Output;

namespace Cashregister.Api.Orders.Models;

public sealed record OrdersPageDto(
    string? Next,
    bool HasNext,
    ImmutableArray<OrderListItemDto> Items
);

public sealed record OrderListItemDto(
    string Id,
    string Number,
    long TotalInCents,
    long? TotalOverrideInCents,
    long Date
)
{
    public static OrderListItemDto From(OrderListItem order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderListItemDto(
            order.Id.Value,
            order.Number.Value,
            order.Total.Value,
            order.TotalOverride?.Value,
            order.Date.Value
        );
    }
}