using System.Collections.Immutable;

using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Pagination;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Orders;

public sealed class FetchOrdersListQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_WithNoAfter_ShouldReturnFirstOrdersInOrder()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        var order1Id = await CreateOrderAsync(articleId, 1);
        var order2Id = await CreateOrderAsync(articleId, 2);
        _ = await CreateOrderAsync(articleId, 3);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(2)
            );

        Assert.Equal(2, result.Length);

        // Verify orders are returned in ascending order by ID (ULID)
        Assert.True(string.Compare(result[0].Id.Value, result[1].Id.Value, StringComparison.Ordinal) < 0);

        // Verify the first two orders are returned
        var expectedIds = new[]
            {
                order1Id,
                order2Id
            }
            .OrderBy(id => id.Value)
            .ToArray();

        Assert.Equal(expectedIds[0].Value, result[0].Id.Value);
        Assert.Equal(expectedIds[1].Value, result[1].Id.Value);
    }

    [Fact]
    public async Task FetchAsync_WithAfter_ShouldReturnOrdersAfterCursorExclusive()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        _ = await CreateOrderAsync(articleId, 1);
        var order2Id = await CreateOrderAsync(articleId, 2);
        var order3Id = await CreateOrderAsync(articleId, 3);
        var order4Id = await CreateOrderAsync(articleId, 4);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(3, order2Id)
            );

        Assert.Equal(2, result.Length);

        Assert.Equal(order3Id.Value, result[0].Id.Value);
        Assert.Equal(order4Id.Value, result[1].Id.Value);
    }

    [Fact]
    public async Task FetchAsync_WithCountLargerThanAvailable_ShouldReturnAllAvailableOrders()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(10)
            );

        Assert.Equal(2, result.Length);

        // Verify orders are returned in ascending order
        Assert.True(string.Compare(result[0].Id.Value, result[1].Id.Value, StringComparison.Ordinal) < 0);
    }

    [Fact]
    public async Task FetchAsync_WithZeroCount_ShouldReturnEmptyArray()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(0)
            );

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAsync_WithNoOrders_ShouldReturnEmptyArray()
    {
        await PrepareEnvironmentAsync();

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(5)
            );

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnCorrectOrderData()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Test Article", 500);

        var orderId = await CreateOrderAsync(articleId, 3);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(1)
            );

        Assert.Single(result);
        Assert.Equal(orderId.Value, result[0].Id.Value);
        Assert.Equal(Cents.From(1500), result[0].Total);
        Assert.NotNull(result[0].Number);
        Assert.True(result[0].Date.Value > 0);
    }

    [Fact]
    public async Task FetchUntilAsync_ShouldReturnOrdersUpToAndIncludingCursor()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        var order1Id = await CreateOrderAsync(articleId, 1);
        var order2Id = await CreateOrderAsync(articleId, 2);
        _ = await CreateOrderAsync(articleId, 3);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchUntilAsync(order2Id)
            );

        Assert.Equal(2, result.Length);
        Assert.Equal(order1Id.Value, result[0].Id.Value);
        Assert.Equal(order2Id.Value, result[1].Id.Value);
    }

    [Fact]
    public async Task FetchUntilAsync_WithCursorAtFirstOrder_ShouldReturnThatOrder()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        var order1Id = await CreateOrderAsync(articleId, 1);
        _ = await CreateOrderAsync(articleId, 2);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchUntilAsync(order1Id)
            );

        Assert.Single(result);
        Assert.Equal(order1Id.Value, result[0].Id.Value);
    }

    [Fact]
    public async Task FetchAsync_ProjectedTotal_ShouldMatchDomainOrderTotal()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 750);

        await CreateOrderAsync(articleId, 4);

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(1)
            );

        Assert.Single(result);

        var projectedTotal = result[0].Total;

        var domainOrder = new Order
        {
            Id = result[0].Id,
            Number = result[0].Number,
            Date = result[0].Date,
            Items =
            [
                new Item
                {
                    Id = Identifier.New(),
                    Article = articleId,
                    Description = "Article A",
                    Price = Cents.From(750),
                    Quantity = 4
                }
            ]
        };

        Assert.Equal(domainOrder.Total(), projectedTotal);
    }

    [Fact]
    public async Task FetchAsync_WithTotalOverride_ShouldReturnOverrideAsTotal()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Override Article", 500);

        var orderId = await CreateOrderWithOverrideAsync(articleId, 4, Cents.From(999L));

        var result =
            await RunScoped<IPaginationQuery<OrderListItem>, ImmutableArray<OrderListItem>>(fetcher =>
                fetcher.FetchAsync(1)
            );

        Assert.Single(result);
        Assert.Equal(orderId.Value, result[0].Id.Value);
        Assert.Equal(Cents.From(999L), result[0].Total);
    }

    private async Task<Identifier> CreateOrderWithOverrideAsync(Identifier articleId, uint quantity,
        Cents totalOverride)
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
                ],
                TotalOverride = totalOverride
            })
        );

        Assert.True(placeOrderResult.Ok);
        return placeOrderResult.Value;
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
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

    private async Task<Identifier> CreateOrderAsync(Identifier articleId, uint quantity)
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