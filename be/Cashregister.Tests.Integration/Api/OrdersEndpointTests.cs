using System.Collections.Immutable;
using System.Net;

using Cashregister.Api.Orders.Models;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
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
            Items = [
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

        var orderSummary = await response.Content.ReadFromJsonAsync<OrderSummaryDto>();

        Assert.NotNull(orderSummary);
        Assert.Equal(placeOrderResult.Value.Value, orderSummary.Id);
        // Note: We can't assert Number, Date, and Total since PlaceOrderTransaction only returns the ID
        // The actual values would depend on the implementation and current time
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
}