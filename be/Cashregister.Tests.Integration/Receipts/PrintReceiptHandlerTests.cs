using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Handlers;
using Cashregister.Application.Receipts.Problems;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Receipts;

public sealed class PrintReceiptHandlerTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task ExecuteAsync_ReturnsProblem_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new RecordingDevice()));

        var result = await RunScoped<IPrintReceiptHandler, Result<Unit>>(handler =>
            handler.ExecuteAsync(Identifier.From("nonexistent-order-id")));

        Assert.True(result.NotOk);
        Assert.IsType<NoSuchOrderPrintDataProblem>(result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_PrintsReceiptProgram_WhenOrderExists()
    {
        var device = new RecordingDevice();

        await PrepareEnvironmentAsync(services => ConfigureDevice(services, device));

        var articleId = await CreateArticleAsync("Coffee", 12345);
        var orderId = await CreateOrderAsync(articleId, 2);

        var result = await RunScoped<IPrintReceiptHandler, Result<Unit>>(handler =>
            handler.ExecuteAsync(orderId));

        Assert.True(result.Ok);
        Assert.Equal(1, device.PrintCount);
        Assert.NotNull(device.PrintedProgram);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesDeviceFailure_WhenDeviceFails()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new FailingDevice()));

        var articleId = await CreateArticleAsync("Coffee", 12345);
        var orderId = await CreateOrderAsync(articleId, 2);

        var result = await RunScoped<IPrintReceiptHandler, Result<Unit>>(handler =>
            handler.ExecuteAsync(orderId));

        Assert.True(result.NotOk);
        Assert.IsType<TestDeviceProblem>(result.Error);
    }

    private static void ConfigureDevice(IServiceCollection services, IDevice device)
    {
        services.RemoveAll<IDevice>();
        services.AddSingleton(device);
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

    private async Task<Identifier> CreateOrderAsync(Identifier articleId, uint quantity)
    {
        var result = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = articleId,
                        Quantity = quantity
                    }
                ]
            }));

        Assert.True(result.Ok);
        return result.Value;
    }

    private sealed class RecordingDevice : IDevice
    {
        public int PrintCount { get; private set; }

        public PrintProgram? PrintedProgram { get; private set; }

        public Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
        {
            PrintCount++;
            PrintedProgram = printProgram;

            return Task.FromResult(Result.Void());
        }
    }

    private sealed class FailingDevice : IDevice
    {
        public Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
        {
            return Task.FromResult(Result.Error(new TestDeviceProblem()));
        }
    }

    private sealed record TestDeviceProblem : Problem;
}