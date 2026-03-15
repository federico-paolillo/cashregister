using System.Collections.Immutable;

namespace Cashregister.Domain;

public sealed class Order
{
    public required Identifier Id { get; init; }

    public required OrderNumber Number { get; init; }

    public required TimeStamp Date { get; init; }

    public required ImmutableArray<Item> Items { get; init; } = [];

    public Cents? TotalOverride { get; init; }

    public Cents Total()
    {
        return TotalOverride ?? Cents.From(Items.Sum(item => item.Price.Value * item.Quantity));
    }
}