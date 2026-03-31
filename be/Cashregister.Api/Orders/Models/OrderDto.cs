using System.Collections.Immutable;

using Cashregister.Domain;

namespace Cashregister.Api.Orders.Models;

public sealed record OrderItemDto(
    string Id,
    string Article,
    string Description,
    long PriceInCents,
    uint Quantity
);

public sealed record OrderDto(
    string Id,
    string Number,
    long Date,
    long TotalInCents,
    long? TotalOverrideInCents,
    ImmutableArray<OrderItemDto> Items
)
{
    public static OrderDto From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        ImmutableArray<OrderItemDto> items =
        [
            .. order.Items.Select(item => new OrderItemDto(
                item.Id.Value,
                item.Article.Value,
                item.Description,
                item.Price.Value,
                item.Quantity
            ))
        ];

        return new OrderDto(
            order.Id.Value,
            order.Number.Value,
            order.Date.Value,
            order.Total().Value,
            order.TotalOverride?.Value,
            items
        );
    }
}