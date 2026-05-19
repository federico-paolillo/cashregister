using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Writes all-time article statistics CSV content.
/// </summary>
public sealed class WriteArticleStatisticsCsvHandler(
    IFetchStatisticsQuery fetchStatisticsQuery
) : IWriteArticleStatisticsCsvHandler
{
    public async Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var statistics = await fetchStatisticsQuery.FetchAsync(cancellationToken);

        await StatisticsCsvWriter.WriteRecordsAsync<ArticleStatisticsItem, ArticleStatisticsItemCsvMap>(
            stream,
            statistics.Articles.Items,
            cancellationToken
        );
    }
}