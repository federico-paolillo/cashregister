namespace Cashregister.Application.Statistics.Handlers;

/// <summary>
/// Writes all-time order-volume statistics as CSV.
/// </summary>
public interface IWriteOrderStatisticsCsvHandler
{
    Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default);
}