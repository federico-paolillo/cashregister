using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Writes all-time order-volume statistics CSV content.
/// </summary>
public sealed class WriteOrderStatisticsCsvHandler(
    IFetchStatisticsQuery fetchStatisticsQuery
) : IWriteOrderStatisticsCsvHandler
{
    public async Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var statistics = await fetchStatisticsQuery.FetchAsync(cancellationToken);

        await StatisticsCsvWriter.WriteRecordsAsync<OrderStatisticsSummary, OrderStatisticsSummaryCsvMap>(
            stream,
            [statistics.Orders],
            cancellationToken
        );
    }
}