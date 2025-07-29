using System.Collections.Immutable;

using Cashregister.Domain;

namespace Cashregister.Application.Orders.Models.Input;

public sealed class OrderRequest
{
    public required ImmutableArray<OrderRequestItem> Items { get; init; } = [];
}

public sealed class OrderRequestItem
{
    public required Identifier Article { get; init; }

    public required uint Quantity { get; init; }
}