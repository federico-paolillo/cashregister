using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Orders;

public sealed class FetchOrderQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IFetchOrderQuery, Order?>(
            q => q.FetchAsync(Identifier.From("nonexistent-order-id"))
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchAsync_ReturnsOrder_WhenOrderExists()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Test Article", 750);

        var orderId = await CreateOrderAsync(articleId, 4);

        var order = await RunScoped<IFetchOrderQuery, Order?>(
            q => q.FetchAsync(orderId)
        );

        Assert.NotNull(order);
        Assert.Equal(orderId.Value, order.Id.Value);
        Assert.NotNull(order.Number);
        Assert.True(order.Date.Value > 0);
        Assert.Equal(Cents.From(3000L), order.Total());
        Assert.Single(order.Items);

        var item = order.Items[0];
        Assert.Equal(articleId.Value, item.Article.Value);
        Assert.Equal("Test Article", item.Description);
        Assert.Equal(Cents.From(750L), item.Price);
        Assert.Equal(4u, item.Quantity);
    }

    [Fact]
    public async Task FetchAsync_ReturnsOrderWithMultipleItems()
    {
        await PrepareEnvironmentAsync();

        var article1Id = await CreateArticleAsync("Article A", 100);
        var article2Id = await CreateArticleAsync("Article B", 200);

        var orderId = await CreateOrderWithMultipleItemsAsync(article1Id, 2, article2Id, 3);

        var order = await RunScoped<IFetchOrderQuery, Order?>(
            q => q.FetchAsync(orderId)
        );

        Assert.NotNull(order);
        Assert.Equal(2, order.Items.Length);
        Assert.Equal(Cents.From(800L), order.Total());

        var itemA = order.Items.Single(i => i.Description == "Article A");
        Assert.Equal(article1Id.Value, itemA.Article.Value);
        Assert.Equal(Cents.From(100L), itemA.Price);
        Assert.Equal(2u, itemA.Quantity);

        var itemB = order.Items.Single(i => i.Description == "Article B");
        Assert.Equal(article2Id.Value, itemB.Article.Value);
        Assert.Equal(Cents.From(200L), itemB.Price);
        Assert.Equal(3u, itemB.Quantity);
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
    {
        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            })
        );

        Assert.True(registerArticleResult.Ok);
        return registerArticleResult.Value;
    }

    private async Task<Identifier> CreateOrderAsync(Identifier articleId, uint quantity)
    {
        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem
                    {
                        Article = articleId,
                        Quantity = quantity
                    }
                ]
            })
        );

        Assert.True(placeOrderResult.Ok);
        return placeOrderResult.Value;
    }

    private async Task<Identifier> CreateOrderWithMultipleItemsAsync(
        Identifier article1Id, uint quantity1,
        Identifier article2Id, uint quantity2)
    {
        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new OrderRequest
            {
                Items =
                [
                    new OrderRequestItem { Article = article1Id, Quantity = quantity1 },
                    new OrderRequestItem { Article = article2Id, Quantity = quantity2 }
                ]
            })
        );

        Assert.True(placeOrderResult.Ok);
        return placeOrderResult.Value;
    }
}
