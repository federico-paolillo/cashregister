using Cashregister.Api.Statistics.Models;
using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Handlers;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Cashregister.Api.Statistics;

/// <summary>
/// Handles statistics HTTP requests.
/// </summary>
internal static class Handlers
{
    private const string CsvContentType = "text/csv; charset=utf-8";

    public static async Task<Ok<StatisticsDto>> GetStatistics(
        IFetchStatisticsQuery fetchStatisticsQuery,
        CancellationToken cancellationToken
    )
    {
        var statistics = await fetchStatisticsQuery.FetchAsync(cancellationToken);

        return TypedResults.Ok(StatisticsDto.From(statistics));
    }

    public static async Task<FileContentHttpResult> GetSalesStatisticsCsv(
        IWriteSalesStatisticsCsvHandler writeSalesStatisticsCsvHandler,
        CancellationToken cancellationToken
    )
    {
        using MemoryStream stream = new();

        await writeSalesStatisticsCsvHandler.ExecuteAsync(stream, cancellationToken);

        return TypedResults.File(
            stream.ToArray(),
            CsvContentType,
            "statistics-sales.csv"
        );
    }
}