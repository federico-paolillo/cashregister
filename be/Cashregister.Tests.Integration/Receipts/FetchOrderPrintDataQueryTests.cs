using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Receipts.Data;
using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Receipts;

public sealed class FetchOrderPrintDataQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IFetchOrderPrintDataQuery, OrderPrintData?>(q =>
            q.FetchAsync(Identifier.From("nonexistent-order-id")));

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchAsync_ReturnsReceiptProjection_WhenOrderExists()
    {
        await PrepareEnvironmentAsync();

        var coffeeId = await CreateArticleAsync("Coffee", 12345);
        var teaId = await CreateArticleAsync("Tea", 67890);
        var orderId = await CreateOrderAsync(coffeeId, 2, teaId, 3);

        var result = await RunScoped<IFetchOrderPrintDataQuery, OrderPrintData?>(q =>
            q.FetchAsync(orderId));

        Assert.NotNull(result);
        Assert.Equal(orderId.Value, result.Id.Value);
        Assert.NotNull(result.Number);
        Assert.True(result.Date.Value > 0);
        Assert.Equal(Cents.From(228360), result.Total);
        Assert.Equal(2, result.Items.Length);
        Assert.Equal("Coffee", result.Items[0].Description);
        Assert.Equal(Cents.From(12345), result.Items[0].Price);
        Assert.Equal(2u, result.Items[0].Quantity);
        Assert.Equal("Tea", result.Items[1].Description);
        Assert.Equal(Cents.From(67890), result.Items[1].Price);
        Assert.Equal(3u, result.Items[1].Quantity);
    }

    [Fact]
    public async Task FetchAsync_UsesTotalOverride_WhenOrderHasOverride()
    {
        await PrepareEnvironmentAsync();

        var coffeeId = await CreateArticleAsync("Coffee", 12345);
        var teaId = await CreateArticleAsync("Tea", 67890);
        var orderId = await CreateOrderAsync(coffeeId, 2, teaId, 3, Cents.From(999));

        var result = await RunScoped<IFetchOrderPrintDataQuery, OrderPrintData?>(q =>
            q.FetchAsync(orderId));

        Assert.NotNull(result);
        Assert.Equal(Cents.From(999), result.Total);
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
        uint secondQuantity,
        Cents? totalOverride = null)
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
                ],
                TotalOverride = totalOverride
            }));

        Assert.True(result.Ok);
        return result.Value;
    }
}