namespace CashRegister.Domain;

public sealed class Item
{
    public required Identifier Id { get; init; }
    
    public required Identifier Article { get; init; }
    
    public required string Description { get; init; }
    
    public required Cents Price { get; init; }
    
    public required uint Quantity { get; init; }
}