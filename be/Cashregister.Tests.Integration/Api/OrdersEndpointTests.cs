using System.Collections.Immutable;
using System.Net;

using Cashregister.Api.Commons.Models;
using Cashregister.Api.Orders.Models;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class OrdersEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task CreateOrder_ReturnsCreated_WhenOrderIsValid()
    {
        await PrepareEnvironmentAsync();

        // First, register an article to use in the order
        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Test article for order",
                Price = Cents.From(1000L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        using var httpClient = CreateHttpClient();

        var orderRequest = new OrderRequestDto(
            [
                new OrderRequestItemDto(
                    registerArticleResult.Value.Value,
                    2u
                )
            ]
        );

        var response = await httpClient.PostAsJsonAsync("/orders", orderRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();

        Assert.NotNull(entityPointer);
        Assert.NotEmpty(entityPointer.Id);
        Assert.NotEmpty(entityPointer.Location);
        Assert.Equal($"/orders/{entityPointer.Id}", entityPointer.Location);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenOrderHasInvalidItems()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var orderRequest = new OrderRequestDto(
            [
                new OrderRequestItemDto(
                    "nonexistent-article-id",
                    1u
                )
            ]
        );

        var response = await httpClient.PostAsJsonAsync("/orders", orderRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrder_WhenOrderExists()
    {
        await PrepareEnvironmentAsync();

        // First, register an article
        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Test article for order",
                Price = Cents.From(500L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        // Create an order using the transaction directly
        var orderRequest = new OrderRequest
        {
            Items =
            [
                new OrderRequestItem
                {
                    Article = registerArticleResult.Value,
                    Quantity = 3
                }
            ]
        };

        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(orderRequest)
        );

        Assert.True(placeOrderResult.Ok);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync($"/orders/{placeOrderResult.Value.Value}");

        Assert.True(response.IsSuccessStatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();

        Assert.NotNull(order);
        Assert.Equal(placeOrderResult.Value.Value, order.Id);
        Assert.NotEmpty(order.Number);
        Assert.True(order.Date > 0);
        Assert.Equal(15m, order.Total);
        Assert.Single(order.Items);
        Assert.Equal("Test article for order", order.Items[0].Description);
        Assert.Equal(5m, order.Items[0].Price);
        Assert.Equal(3u, order.Items[0].Quantity);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrderWithMultipleItems_WhenOrderHasMultipleItems()
    {
        await PrepareEnvironmentAsync();

        var article1Id = await CreateArticleForOrderAsync("Article A", 1000L);
        var article2Id = await CreateArticleForOrderAsync("Article B", 2500L);

        var orderRequest = new OrderRequest
        {
            Items =
            [
                new OrderRequestItem
                {
                    Article = article1Id,
                    Quantity = 2
                },
                new OrderRequestItem
                {
                    Article = article2Id,
                    Quantity = 1
                }
            ]
        };

        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(orderRequest)
        );

        Assert.True(placeOrderResult.Ok);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync($"/orders/{placeOrderResult.Value.Value}");

        Assert.True(response.IsSuccessStatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();

        Assert.NotNull(order);
        Assert.Equal(2, order.Items.Length);
        Assert.Equal(45m, order.Total);

        var itemA = order.Items.Single(i => i.Description == "Article A");
        Assert.Equal(10m, itemA.Price);
        Assert.Equal(2u, itemA.Quantity);
        Assert.Equal(article1Id.Value, itemA.Article);

        var itemB = order.Items.Single(i => i.Description == "Article B");
        Assert.Equal(25m, itemB.Price);
        Assert.Equal(1u, itemB.Quantity);
        Assert.Equal(article2Id.Value, itemB.Article);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/orders/nonexistent-order-id");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithMultipleItems_ReturnsCreated()
    {
        await PrepareEnvironmentAsync();

        // Register multiple articles
        var article1Result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article 1",
                Price = Cents.From(1000L)
            })
        );

        var article2Result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article 2",
                Price = Cents.From(2500L)
            })
        );

        Assert.True(article1Result.Ok);
        Assert.True(article2Result.Ok);

        using var httpClient = CreateHttpClient();

        var orderRequest = new OrderRequestDto(
            [
                new OrderRequestItemDto(article1Result.Value.Value, 2u),
                new OrderRequestItemDto(article2Result.Value.Value, 1u)
            ]
        );

        var response = await httpClient.PostAsJsonAsync("/orders", orderRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetOrders_ReturnsEmptyList_WhenNoOrdersExist()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/orders");

        Assert.True(response.IsSuccessStatusCode);

        var ordersPage = await response.Content.ReadFromJsonAsync<OrdersPageDto>();

        Assert.NotNull(ordersPage);
        Assert.Empty(ordersPage.Items);
        Assert.False(ordersPage.HasNext);
        Assert.Null(ordersPage.Next);
    }

    [Fact]
    public async Task GetOrders_ReturnsOrders_WhenOrdersExist()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleForOrderAsync("Test article", 500L);

        var orderId = await PlaceOrderAsync(articleId, 3);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/orders");

        Assert.True(response.IsSuccessStatusCode);

        var ordersPage = await response.Content.ReadFromJsonAsync<OrdersPageDto>();

        Assert.NotNull(ordersPage);
        Assert.Single(ordersPage.Items);
        Assert.Equal(orderId.Value, ordersPage.Items[0].Id);
        Assert.NotEmpty(ordersPage.Items[0].Number);
        Assert.Equal(15m, ordersPage.Items[0].Total);
        Assert.True(ordersPage.Items[0].Date > 0);
    }

    [Fact]
    public async Task GetOrders_ReturnsOrders_WhenOrdersExist_WithPagination()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleForOrderAsync("Test article", 100L);

        var order1Id = await PlaceOrderAsync(articleId, 1);
        var order2Id = await PlaceOrderAsync(articleId, 2);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/orders?pageSize=1");

        Assert.True(response.IsSuccessStatusCode);

        var ordersPage1 = await response.Content.ReadFromJsonAsync<OrdersPageDto>();

        Assert.NotNull(ordersPage1);
        Assert.NotNull(ordersPage1.Next);
        Assert.True(ordersPage1.HasNext);
        Assert.Single(ordersPage1.Items);
        Assert.Equal(order1Id.Value, ordersPage1.Items[0].Id);

        response = await httpClient.GetAsync($"/orders?pageSize=1&after={ordersPage1.Next}");

        Assert.True(response.IsSuccessStatusCode);

        var ordersPage2 = await response.Content.ReadFromJsonAsync<OrdersPageDto>();

        Assert.NotNull(ordersPage2);
        Assert.Null(ordersPage2.Next);
        Assert.False(ordersPage2.HasNext);
        Assert.Single(ordersPage2.Items);
        Assert.Equal(order2Id.Value, ordersPage2.Items[0].Id);
    }

    [Fact]
    public async Task GetOrders_WithUntil_ReturnsAccumulatedViewPlusOneMorePage()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleForOrderAsync("Test article", 100L);

        var order1Id = await PlaceOrderAsync(articleId, 1);
        var order2Id = await PlaceOrderAsync(articleId, 2);
        await PlaceOrderAsync(articleId, 3);

        using var httpClient = CreateHttpClient();

        // Get page 1 to obtain the Next cursor
        var page1Response = await httpClient.GetAsync("/orders?pageSize=1");
        Assert.True(page1Response.IsSuccessStatusCode);

        var page1 = await page1Response.Content.ReadFromJsonAsync<OrdersPageDto>();
        Assert.NotNull(page1);
        Assert.NotNull(page1.Next);

        // Use the Next cursor from page 1 as the until parameter
        var response = await httpClient.GetAsync($"/orders?pageSize=1&until={page1.Next}");

        Assert.True(response.IsSuccessStatusCode);

        var page = await response.Content.ReadFromJsonAsync<OrdersPageDto>();

        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Length);
        Assert.Equal(order1Id.Value, page.Items[0].Id);
        Assert.Equal(order2Id.Value, page.Items[1].Id);
        Assert.True(page.HasNext);
        Assert.Equal(order2Id.Value, page.Next);
    }

    [Fact]
    public async Task GetOrders_WithBothAfterAndUntil_ReturnsBadRequest()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/orders?after=somecursor&until=anothercursor");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Identifier> CreateArticleForOrderAsync(string description, long priceInCents)
    {
        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            })
        );

        Assert.True(registerArticleResult.Ok);
        return registerArticleResult.Value;
    }

    private async Task<Identifier> PlaceOrderAsync(Identifier articleId, uint quantity)
    {
        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
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
            })
        );

        Assert.True(placeOrderResult.Ok);
        return placeOrderResult.Value;
    }
}