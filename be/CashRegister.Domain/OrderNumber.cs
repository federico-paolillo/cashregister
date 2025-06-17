namespace CashRegister.Domain;

public sealed record OrderNumber(string Value)
{
    public static OrderNumber From(long rawOrderNumber)
    {
        return new OrderNumber($"{rawOrderNumber:D20}");
    }
}