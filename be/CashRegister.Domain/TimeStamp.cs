namespace CashRegister.Domain;

public sealed record TimeStamp(long Value)
{
    public static TimeStamp Now()
    {
        var nowstamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return new TimeStamp(nowstamp);
    }

    public static TimeStamp From(long rawTimestamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rawTimestamp);

        return new TimeStamp(rawTimestamp);
    }
}