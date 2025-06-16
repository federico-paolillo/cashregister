using CashRegister.Domain;

namespace CashRegister.Application.Orders.Models.Output;

public sealed class OrderSummary
{
    public required Identifier Id { get; init; }
    
    public required TimeStamp Date { get; init; }

    public required Cents Total { get; init; }
}