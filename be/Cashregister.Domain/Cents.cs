namespace Cashregister.Domain;

public sealed record Cents(long Value)
{
    public static Cents From(long total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        return new Cents(total);
    }
}