using Cashregister.Application.Statistics.Models.Output;

using CsvHelper.Configuration;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

/// <summary>
/// Maps article statistics output models to CSV columns.
/// </summary>
public sealed class ArticleStatisticsItemCsvMap : ClassMap<ArticleStatisticsItem>
{
    public ArticleStatisticsItemCsvMap()
    {
        Map(x => x.ArticleId)
            .Name("ArticleId")
            .Convert(args => args.Value.ArticleId.Value);
        Map(x => x.Description)
            .Name("Description");
        Map(x => x.SoldUnits)
            .Name("SoldUnits");
        Map(x => x.OrdersIncluded)
            .Name("OrdersIncluded");
        Map(x => x.VolumeInCents)
            .Name("Volume")
            .Convert(args => StatisticsCsvWriter.FormatPrice(args.Value.VolumeInCents));
    }
}