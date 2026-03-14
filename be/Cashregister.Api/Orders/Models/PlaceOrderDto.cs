using System.Collections.Immutable;

namespace Cashregister.Api.Orders.Models;

public sealed record OrderRequestDto(
  ImmutableArray<OrderRequestItemDto> Items
);

public sealed record OrderRequestItemDto(
  string Article,
  uint Quantity
);