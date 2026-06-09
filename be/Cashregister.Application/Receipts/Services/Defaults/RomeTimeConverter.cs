using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Services.Defaults;

/// <summary>
///     Converts stored UTC timestamps to Rome local time without requiring system time-zone data.
/// </summary>
public sealed class RomeTimeConverter
{
    private static readonly TimeSpan CentralEuropeanTimeOffset = TimeSpan.FromHours(1);
    private static readonly TimeSpan CentralEuropeanSummerTimeOffset = TimeSpan.FromHours(2);

    public DateTimeOffset ToRomeTime(TimeStamp timestamp)
    {
        ArgumentNullException.ThrowIfNull(timestamp);

        var utcTime = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);
        var offset = IsCentralEuropeanSummerTime(utcTime)
            ? CentralEuropeanSummerTimeOffset
            : CentralEuropeanTimeOffset;

        return utcTime.ToOffset(offset);
    }

    private static bool IsCentralEuropeanSummerTime(DateTimeOffset utcTime)
    {
        var summerTimeStarts = LastSundayOfMonthAtOneUtc(utcTime.Year, 3);
        var summerTimeEnds = LastSundayOfMonthAtOneUtc(utcTime.Year, 10);

        return utcTime >= summerTimeStarts && utcTime < summerTimeEnds;
    }

    private static DateTimeOffset LastSundayOfMonthAtOneUtc(int year, int month)
    {
        var date = new DateTimeOffset(
            year,
            month,
            DateTime.DaysInMonth(year, month),
            1,
            0,
            0,
            TimeSpan.Zero);

        while (date.DayOfWeek is not DayOfWeek.Sunday)
        {
            date = date.AddDays(-1);
        }

        return date;
    }
}