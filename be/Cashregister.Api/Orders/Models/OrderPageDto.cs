using System.Collections.Immutable;

using Cashregister.Application.Orders.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Api.Orders.Models;

public sealed record OrdersPageDto(
  string? Next,
  bool HasNext,
  ImmutableArray<OrderListItemDto> Items
);

public sealed record OrderListItemDto(
  string Id,
  string Number,
  decimal Total,
  long Date
)
{
    public static OrderListItemDto From(OrderListItem order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderListItemDto(
          order.Id.Value,
          OrderNumber.From(order.Number).Value,
          Cents.From(order.Total).AsPayableMoney(),
          order.Date
        );
    }
}
