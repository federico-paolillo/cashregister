using System.Collections.Immutable;
using System.Globalization;

using Cashregister.Application.Receipts.Data;
using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Application.Receipts.Problems;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Application.Receipts.Services.Defaults;

/// <summary>
///     Default receipt print program service that builds the order receipt template.
/// </summary>
public sealed class ReceiptPrintProgramService(
    IFetchOrderPrintDataQuery fetchOrderPrintDataQuery,
    RomeTimeConverter romeTimeConverter
) : IReceiptPrintProgramService
{
    public async Task<Result<ImmutableArray<PrintProgram>>> BuildAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        var orderPrintData = await fetchOrderPrintDataQuery.FetchAsync(orderId);

        if (orderPrintData is null)
        {
            return Result.Error<ImmutableArray<PrintProgram>>(new NoSuchOrderPrintDataProblem(orderId));
        }

        return Result.Ok(Build(orderPrintData));
    }

    private ImmutableArray<PrintProgram> Build(OrderPrintData order)
    {
        var programs = ImmutableArray.CreateBuilder<PrintProgram>();

        programs.Add(BuildOverview(order));

        foreach (var item in order.Items)
        {
            if (!item.PrintDetailReceipt)
            {
                continue;
            }

            for (var i = 0U; i < item.Quantity; i++)
            {
                programs.Add(BuildItemReceipt(order, item));
            }
        }

        return programs.ToImmutable();
    }

    private PrintProgram BuildOverview(OrderPrintData order)
    {
        var builder = new PrintProgramBuilder()
            .FontSize(1)
            .Align(Alignment.Left)
            .PrintLine($"ORDER {order.Number.Value}")
            .LineFeed()
            .PrintLine("---")
            .LineFeed();

        foreach (var item in order.Items)
        {
            builder.PrintLine(
                $"{item.Quantity}x {item.Description} @ {FormatPrice(item.Price)} = {FormatPrice(item.Total())}");
        }

        return builder
            .LineFeed()
            .FontSize(2)
            .PrintLine($"Total: {FormatPrice(order.Total)}")
            .FontSize(1)
            .LineFeed()
            .PrintLine("---")
            .LineFeed()
            .PrintLine($"Order ID: {order.Id.Value}")
            .PrintLine($"Date: {FormatDate(order.Date)}")
            .Build();
    }

    private PrintProgram BuildItemReceipt(OrderPrintData order, OrderPrintDataItem item)
    {
        return new PrintProgramBuilder()
            .Align(Alignment.Left)
            .FontSize(3)
            .PrintLine(item.Description)
            .LineFeed()
            .FontSize(1)
            .PrintLine($"Order: {order.Number.Value}")
            .PrintLine($"Order ID: {order.Id.Value}")
            .PrintLine($"Date: {FormatDate(order.Date)}")
            .Build();
    }

    private string FormatDate(TimeStamp date)
    {
        return romeTimeConverter
            .ToRomeTime(date)
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static string FormatPrice(Cents cents)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{cents.Value / 100}.{cents.Value % 100:00}");
    }
}