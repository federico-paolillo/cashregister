using CashRegister.Domain;

namespace CashRegister.Application.Orders.Models.Input;

public sealed class OrderRequest
{
    public required OrderRequestItem[] Items { get; init; } = [];
}

public sealed class OrderRequestItem
{
    public required Identifier Article { get; init; }
    
    public required int Quantity  { get; init; }
}