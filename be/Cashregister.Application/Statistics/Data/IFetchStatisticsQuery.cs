using Cashregister.Application.Statistics.Models.Output;

namespace Cashregister.Application.Statistics.Data;

/// <summary>
/// Fetches all-time sales statistics.
/// </summary>
public interface IFetchStatisticsQuery
{
    Task<StatisticsReport> FetchAsync(CancellationToken cancellationToken = default);
}