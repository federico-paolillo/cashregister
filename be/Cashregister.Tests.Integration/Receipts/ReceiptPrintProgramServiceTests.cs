using System.Globalization;

using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Data;
using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Application.Receipts.Problems;
using Cashregister.Application.Receipts.Services;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Receipts;

public sealed class ReceiptPrintProgramServiceTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task BuildAsync_ReturnsProblem_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IReceiptPrintProgramService, Result<PrintProgram>>(svc =>
            svc.BuildAsync(Identifier.From("nonexistent-order-id")));

        Assert.True(result.NotOk);
        Assert.IsType<NoSuchOrderPrintDataProblem>(result.Error);
    }

    [Fact]
    public async Task BuildAsync_ReturnsReceiptPrintProgram_WhenOrderExists()
    {
        await PrepareEnvironmentAsync();

        var coffeeId = await CreateArticleAsync("Coffee", 12345);
        var teaId = await CreateArticleAsync("Tea", 67890);
        var orderId = await CreateOrderAsync(coffeeId, 2, teaId, 3);
        var orderPrintData = await FetchOrderPrintDataAsync(orderId);

        var result = await RunScoped<IReceiptPrintProgramService, Result<PrintProgram>>(svc =>
            svc.BuildAsync(orderId));

        Assert.True(result.Ok);

        var markdown = Render(result.Value);

        Assert.Contains($"ORDER {orderPrintData.Number.Value}", markdown, StringComparison.Ordinal);
        Assert.Contains("2x Coffee", markdown, StringComparison.Ordinal);
        Assert.Contains("3x Tea", markdown, StringComparison.Ordinal);
        Assert.Contains($"Order ID: {orderId.Value}", markdown, StringComparison.Ordinal);
        Assert.Contains($"Date: {FormatDate(orderPrintData.Date)}", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("12345", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("67890", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("Total", markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Price", markdown, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<OrderPrintData> FetchOrderPrintDataAsync(Identifier orderId)
    {
        var orderPrintData = await RunScoped<IFetchOrderPrintDataQuery, OrderPrintData?>(q =>
            q.FetchAsync(orderId));

        Assert.NotNull(orderPrintData);
        return orderPrintData;
    }

    private static string Render(PrintProgram program)
    {
        var bytesResult = new BinaryEncoder().Encode(program);

        Assert.True(bytesResult.Ok);

        var emulator = new PrinterEmulator(new InstructionDecoder(), new InstructionExecutor());
        var historyResult = emulator.Emulate(bytesResult.Value);

        Assert.True(historyResult.Ok);

        return new MarkdownRenderer().Render(historyResult.Value[^1].Receipt);
    }

    private static string FormatDate(TimeStamp date)
    {
        return DateTimeOffset
            .FromUnixTimeSeconds(date.Value)
            .UtcDateTime
            .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
    {
        var result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            }));

        Assert.True(result.Ok);
        return result.Value;
    }

    private async Task<Identifier> CreateOrderAsync(
        Identifier firstArticleId,
        uint firstQuantity,
        Identifier secondArticleId,
        uint secondQuantity)
    {
        var result = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = firstArticleId,
                        Quantity = firstQuantity
                    },
                    new OrderRequestItem
                    {
                        Article = secondArticleId,
                        Quantity = secondQuantity
                    }
                ]
            }));

        Assert.True(result.Ok);
        return result.Value;
    }
}