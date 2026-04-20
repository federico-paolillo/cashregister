using System.Net;

using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class OrderReceiptPrintingEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task PrintOrderReceipt_ReturnsNoContent_WhenOrderExists()
    {
        var device = new RecordingDevice();

        await PrepareEnvironmentAsync(services => ConfigureDevice(services, device));

        var articleId = await CreateArticleAsync("Coffee", 12345);
        var orderId = await CreateOrderAsync(articleId, 2);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync($"/orders/{orderId.Value}/print", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, device.PrintCount);
        Assert.NotNull(device.PrintedProgram);
    }

    [Fact]
    public async Task PrintOrderReceipt_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new RecordingDevice()));

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync("/orders/nonexistent-order-id/print", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PrintOrderReceipt_ReturnsInternalServerError_WhenDeviceFails()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new FailingDevice()));

        var articleId = await CreateArticleAsync("Coffee", 12345);
        var orderId = await CreateOrderAsync(articleId, 2);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync($"/orders/{orderId.Value}/print", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ApiPrefixedPrintOrderReceiptRoute_ReturnsNotFound()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevice(services, new RecordingDevice()));

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync("/api/orders/nonexistent-order-id/print", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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