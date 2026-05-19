namespace Cashregister.Application.Statistics.Handlers;

/// <summary>
/// Writes all-time article statistics as CSV.
/// </summary>
public interface IWriteArticleStatisticsCsvHandler
{
    Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default);
}