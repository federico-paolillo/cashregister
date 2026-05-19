using Cashregister.Application.Statistics.Models.Output;

using CsvHelper.Configuration;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Maps order statistics output models to CSV columns.
/// </summary>
public sealed class OrderStatisticsSummaryCsvMap : ClassMap<OrderStatisticsSummary>
{
    public OrderStatisticsSummaryCsvMap()
    {
        Map(x => x.OrderCount)
            .Name("OrderCount");
        Map(x => x.NominalVolumeInCents)
            .Name("NominalVolume")
            .Convert(args => StatisticsCsvWriter.FormatPrice(args.Value.NominalVolumeInCents));
        Map(x => x.RealVolumeInCents)
            .Name("RealVolume")
            .Convert(args => StatisticsCsvWriter.FormatPrice(args.Value.RealVolumeInCents));
        Map(x => x.DeltaInCents)
            .Name("Delta")
            .Convert(args => StatisticsCsvWriter.FormatPrice(args.Value.DeltaInCents));
    }
}