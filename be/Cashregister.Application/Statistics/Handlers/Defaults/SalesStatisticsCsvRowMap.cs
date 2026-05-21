using System.Globalization;

using Cashregister.Application.Statistics.Models.Output;

using CsvHelper.Configuration;

namespace Cashregister.Application.Statistics.Handlers.Defaults;

public sealed record SalesStatisticsCsvRecord(
    string OrderId,
    string OrderNumber,
    long OrderDateUnixSeconds,
    string OrderDateUtc,
    string OrderItemId,
    string ArticleId,
    string CurrentArticleDescription,
    string SoldDescription,
    bool ArticleRetired,
    long UnitPriceInCents,
    long Quantity,
    long? OrderTotalOverrideInCents
);

/// <summary>
/// Maps raw sales statistics rows to CSV columns.
/// </summary>
public sealed class SalesStatisticsCsvRowMap : ClassMap<SalesStatisticsCsvRecord>
{
    public SalesStatisticsCsvRowMap()
    {
        Map(x => x.OrderId)
            .Name("OrderId");
        Map(x => x.OrderNumber)
            .Name("OrderNumber");
        Map(x => x.OrderDateUnixSeconds)
            .Name("OrderDateUnixSeconds");
        Map(x => x.OrderDateUtc)
            .Name("OrderDateUtc");
        Map(x => x.OrderItemId)
            .Name("OrderItemId");
        Map(x => x.ArticleId)
            .Name("ArticleId");
        Map(x => x.CurrentArticleDescription)
            .Name("CurrentArticleDescription");
        Map(x => x.SoldDescription)
            .Name("SoldDescription");
        Map(x => x.ArticleRetired)
            .Name("ArticleRetired");
        Map(x => x.UnitPriceInCents)
            .Name("UnitPriceInCents");
        Map(x => x.Quantity)
            .Name("Quantity");
        Map(x => x.OrderTotalOverrideInCents)
            .Name("OrderTotalOverrideInCents");
    }

    public static SalesStatisticsCsvRecord From(SalesStatisticsCsvRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return new SalesStatisticsCsvRecord(
            row.OrderId.Value,
            row.OrderNumber.Value,
            row.OrderDate.Value,
            DateTimeOffset
                .FromUnixTimeSeconds(row.OrderDate.Value)
                .UtcDateTime
                .ToString("O", CultureInfo.InvariantCulture),
            row.OrderItemId.Value,
            row.ArticleId.Value,
            row.CurrentArticleDescription,
            row.SoldDescription,
            row.ArticleRetired,
            row.UnitPriceInCents,
            row.Quantity,
            row.OrderTotalOverrideInCents
        );
    }
}