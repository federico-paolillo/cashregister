using Cashregister.Application.Receipts.Services.Defaults;
using Cashregister.Domain;

namespace Cashregister.Tests.Integration.Receipts;

public sealed class RomeTimeConverterTests
{
    private readonly RomeTimeConverter _converter = new();

    [Fact]
    public void ToRomeTime_UsesCentralEuropeanTime_InWinter()
    {
        var result = _converter.ToRomeTime(TimestampFromUtc(2026, 1, 15, 12, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 1, 15, 13, 0, 0, TimeSpan.FromHours(1)), result);
    }

    [Fact]
    public void ToRomeTime_UsesCentralEuropeanSummerTime_InSummer()
    {
        var result = _converter.ToRomeTime(TimestampFromUtc(2026, 6, 15, 12, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 6, 15, 14, 0, 0, TimeSpan.FromHours(2)), result);
    }

    [Fact]
    public void ToRomeTime_StartsSummerTime_AtMarchTransition()
    {
        var beforeTransition = _converter.ToRomeTime(TimestampFromUtc(2026, 3, 29, 0, 59, 59));
        var transition = _converter.ToRomeTime(TimestampFromUtc(2026, 3, 29, 1, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 3, 29, 1, 59, 59, TimeSpan.FromHours(1)), beforeTransition);
        Assert.Equal(new DateTimeOffset(2026, 3, 29, 3, 0, 0, TimeSpan.FromHours(2)), transition);
    }

    [Fact]
    public void ToRomeTime_EndsSummerTime_AtOctoberTransition()
    {
        var beforeTransition = _converter.ToRomeTime(TimestampFromUtc(2026, 10, 25, 0, 59, 59));
        var transition = _converter.ToRomeTime(TimestampFromUtc(2026, 10, 25, 1, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 10, 25, 2, 59, 59, TimeSpan.FromHours(2)), beforeTransition);
        Assert.Equal(new DateTimeOffset(2026, 10, 25, 2, 0, 0, TimeSpan.FromHours(1)), transition);
    }

    private static TimeStamp TimestampFromUtc(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second)
    {
        return TimeStamp.From(
            new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero)
                .ToUnixTimeSeconds());
    }
}