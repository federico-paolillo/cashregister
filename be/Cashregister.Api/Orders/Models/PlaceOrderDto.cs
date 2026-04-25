using System.Collections.Immutable;

namespace Cashregister.Api.Orders.Models;

public sealed record OrderRequestDto(
    ImmutableArray<OrderRequestItemDto> Items,
    long? TotalOverrideInCents = null
);

public sealed record OrderRequestItemDto(
    string Article,
    uint Quantity
);