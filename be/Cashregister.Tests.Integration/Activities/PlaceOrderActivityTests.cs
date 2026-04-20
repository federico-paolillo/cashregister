using Cashregister.Activities;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Handlers;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Problems;
using Cashregister.Application.Pagination;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Activities;

public sealed class PlaceOrderActivityTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task PlaceOrderAsync_ReturnsOrderAndPrintsReceipt_WhenOrderIsValid()
    {
        var device = new RecordingDevice();

        await PrepareEnvironmentAsync(services => ConfigureDevice(services, device));

        var articleId = await CreateArticleAsync("Coffee", 12345);

        var result = await RunScoped<PlaceOrderActivity, Result<Order>>(activity =>
            activity.PlaceOrderAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = articleId,
                        Quantity = 2
                    }
                ]
            }));

        Assert.True(result.Ok);
        Assert.Equal(1, device.PrintCount);
        Assert.NotNull(device.PrintedProgram);
        Assert.Equal(articleId.Value, result.Value.Items.Single().Article.Value);
    }

    [Fact]
    public async Task PlaceOrderAsync_ReturnsProblemAndDoesNotPrint_WhenOrderIsInvalid()
    {
        var device = new RecordingDevice();

        await PrepareEnvironmentAsync(services => ConfigureDevice(services, device));

        var result = await RunScoped<PlaceOrderActivity, Result<Order>>(activity =>
            activity.PlaceOrderAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = Identifier.From("nonexistent-article-id"),
                        Quantity = 1
                    }
                ]
            }));

        Assert.True(result.NotOk);
        Assert.IsType<OrderRequestIsMissingSomeArticles>(result.Error);
        Assert.Equal(0, device.PrintCount);
    }

    [Fact]
    public async Task PlaceOrderAsync_ReturnsDeviceFailureAndKeepsOrder_WhenPrintingFails()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new FailingDevice()));

        var articleId = await CreateArticleAsync("Coffee", 12345);

        var result = await RunScoped<PlaceOrderActivity, Result<Order>>(activity =>
            activity.PlaceOrderAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = articleId,
                        Quantity = 2
                    }
                ]
            }));

        var ordersPageResult = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 10
            }));

        Assert.True(result.NotOk);
        Assert.IsType<TestDeviceProblem>(result.Error);
        Assert.True(ordersPageResult.Ok);
        Assert.Single(ordersPageResult.Value.Items);
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