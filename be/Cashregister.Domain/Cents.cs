namespace Cashregister.Domain;

public sealed record Cents(long Value)
{
    public static Cents From(long total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        var rounded = (total + 2L) / 5L * 5L;

        return new Cents(rounded);
    }
}