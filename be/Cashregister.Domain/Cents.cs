namespace Cashregister.Domain;

public sealed record Cents(long Value)
{
    public static Cents From(long total)
    {
        return new Cents(total);
    }

    public decimal AsPayableMoney()
    {
        var integerValue = Value / 100m;

        // 1/20 is 0.05. We want to round to the nearest 0.05
        // We multiply by 20 to get the number of 1.0 units then we round and look for the 0.05 units

        var rounded = Math.Round(integerValue * 20, MidpointRounding.AwayFromZero) / 20m;

        return rounded;
    }
}