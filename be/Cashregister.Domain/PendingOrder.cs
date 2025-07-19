using System.Collections.Immutable;

namespace Cashregister.Domain;

public sealed class PendingOrder
{
    public required Identifier Id { get; init; }

    public required TimeStamp Date { get; init; }

    public required ImmutableArray<Item> Items { get; init; } = [];
}