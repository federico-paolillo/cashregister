namespace CashRegister.Domain;

public sealed class PendingOrder
{
    public required Identifier Id { get; init; }
    
    public required TimeStamp Date { get; init; }

    public required Item[] Items { get; init; } = [];
}