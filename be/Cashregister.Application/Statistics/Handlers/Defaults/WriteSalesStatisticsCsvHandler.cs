using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Writes raw all-time sales statistics CSV content.
/// </summary>
public sealed class WriteSalesStatisticsCsvHandler(
    IFetchStatisticsQuery fetchStatisticsQuery
) : IWriteSalesStatisticsCsvHandler
{
    public async Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var statistics = await fetchStatisticsQuery.FetchAsync(cancellationToken);

        var records = statistics.SalesRows.Select(SalesStatisticsCsvRowMap.From);

        await StatisticsCsvWriter.WriteRecordsAsync<SalesStatisticsCsvRecord, SalesStatisticsCsvRowMap>(
            stream,
            records,
            cancellationToken
        );
    }
}