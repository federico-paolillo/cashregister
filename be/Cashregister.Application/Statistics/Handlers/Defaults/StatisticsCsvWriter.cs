using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Writes statistics CSV records to caller-owned streams.
/// </summary>
internal static class StatisticsCsvWriter
{
    public static async Task WriteRecordsAsync<TRecord, TMap>(
        Stream stream,
        IEnumerable<TRecord> records,
        CancellationToken cancellationToken
    )
        where TMap : ClassMap<TRecord>
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(records);

        using StreamWriter streamWriter = new(stream, new UTF8Encoding(false), leaveOpen: true);
        using CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.Context.RegisterClassMap<TMap>();

        await csvWriter.WriteRecordsAsync(records, cancellationToken);
        await streamWriter.FlushAsync(cancellationToken);
    }
}