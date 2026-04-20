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
    IFetchOrderPrintDataQuery fetchOrderPrintDataQuery
) : IReceiptPrintProgramService
{
    public async Task<Result<PrintProgram>> BuildAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        var orderPrintData = await fetchOrderPrintDataQuery.FetchAsync(orderId);

        if (orderPrintData is null)
        {
            return Result.Error<PrintProgram>(new NoSuchOrderPrintDataProblem(orderId));
        }

        return Result.Ok(Build(orderPrintData));
    }

    private static PrintProgram Build(OrderPrintData order)
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

    private static string FormatDate(TimeStamp date)
    {
        return DateTimeOffset
            .FromUnixTimeSeconds(date.Value)
            .UtcDateTime
            .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }
}