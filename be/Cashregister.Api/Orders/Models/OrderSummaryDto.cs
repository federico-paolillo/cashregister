using Cashregister.Application.Orders.Models.Output;

namespace Cashregister.Api.Orders.Models;

public sealed record OrderSummaryDto(
  string Id,
  string Number,
  long Date,
  decimal Total
)
{
    public static OrderSummaryDto From(OrderSummary orderSummary)
    {
        ArgumentNullException.ThrowIfNull(orderSummary);

        return new OrderSummaryDto(
          orderSummary.Id.Value,
          orderSummary.Number.Value,
          orderSummary.Date.Value,
          orderSummary.Total.AsPayableMoney()
        );
    }
}