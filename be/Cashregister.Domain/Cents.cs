namespace Cashregister.Domain;

public sealed record Cents(long Value)
{
    public static Cents From(long total)
    {
        return new Cents(total);
    }
}