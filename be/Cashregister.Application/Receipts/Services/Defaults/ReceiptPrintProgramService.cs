using System.Collections.Immutable;
using System.Globalization;

using Cashregister.Application.Receipts.Data;
using Cashregister.Application.Receipts.Models;
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
    ReceiptModeStore receiptModeStore
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

        return Result.Ok(Build(orderPrintData, receiptModeStore.Current));
    }

    private static ImmutableArray<PrintProgram> Build(OrderPrintData order, ReceiptMode receiptMode)
    {
        return receiptMode switch
        {
            ReceiptMode.Normal => [BuildNormal(order)],
            ReceiptMode.Detail => BuildDetail(order),
            _ => throw new ArgumentOutOfRangeException(nameof(receiptMode), receiptMode, null)
        };
    }

    private static ImmutableArray<PrintProgram> BuildDetail(OrderPrintData order)
    {
        var programs = ImmutableArray.CreateBuilder<PrintProgram>();

        programs.Add(BuildDetailOverview(order));

        foreach (var item in order.Items)
        {
            for (var i = 0U; i < item.Quantity; i++)
            {
                programs.Add(BuildItemReceipt(order, item));
            }
        }

        return programs.ToImmutable();
    }

    private static PrintProgram BuildNormal(OrderPrintData order)
    {
        var builder = new PrintProgramBuilder()
            .Align(Alignment.Center)
            .BoldOn()
            .PrintLine($"ORDER {order.Number.Value}")
            .BoldOff()
            .Align(Alignment.Left)
            .LineFeed();

        foreach (var item in order.Items)
        {
            builder.PrintLine($"{item.Quantity}x {item.Description}");
        }

        return builder
            .LineFeed()
            .PrintLine($"Order ID: {order.Id.Value}")
            .PrintLine($"Date: {FormatDate(order.Date)}")
            .Build();
    }

    private static PrintProgram BuildDetailOverview(OrderPrintData order)
    {
        var builder = new PrintProgramBuilder()
            .Align(Alignment.Center)
            .BoldOn()
            .PrintLine($"ORDER {order.Number.Value}")
            .BoldOff()
            .Align(Alignment.Left)
            .LineFeed();

        foreach (var item in order.Items)
        {
            builder.PrintLine(
                $"{item.Quantity}x {item.Description} @ {FormatPrice(item.Price)} = {FormatPrice(item.Total())}");
        }

        return builder
            .LineFeed()
            .PrintLine($"Total: {FormatPrice(order.Total)}")
            .LineFeed()
            .PrintLine($"Order ID: {order.Id.Value}")
            .PrintLine($"Date: {FormatDate(order.Date)}")
            .Build();
    }

    private static PrintProgram BuildItemReceipt(OrderPrintData order, OrderPrintDataItem item)
    {
        return new PrintProgramBuilder()
            .Align(Alignment.Center)
            .BoldOn()
            .PrintLine($"ORDER {order.Number.Value}")
            .BoldOff()
            .Align(Alignment.Left)
            .LineFeed()
            .PrintLine(item.Description)
            .LineFeed()
            .PrintLine($"Order ID: {order.Id.Value}")
            .PrintLine($"Date: {FormatDate(order.Date)}")
            .Build();
    }

    private static string FormatDate(TimeStamp date)
    {
        return DateTimeOffset
            .FromUnixTimeSeconds(date.Value)
            .UtcDateTime
            .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }

    private static string FormatPrice(Cents cents)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{cents.Value / 100}.{cents.Value % 100:00}");
    }
}