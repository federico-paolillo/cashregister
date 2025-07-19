using System.Collections.Immutable;

namespace Cashregister.Domain;

public class Order
{
    public required Identifier Id { get; init; }

    public required OrderNumber Number { get; init; }

    public required TimeStamp Date { get; init; }

    public required ImmutableArray<Item> Items { get; init; } = [];
}