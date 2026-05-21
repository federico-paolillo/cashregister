namespace Cashregister.Application.Statistics.Handlers;

/// <summary>
/// Writes raw all-time sales statistics as CSV.
/// </summary>
public interface IWriteSalesStatisticsCsvHandler
{
    Task ExecuteAsync(Stream stream, CancellationToken cancellationToken = default);
}