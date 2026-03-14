using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Handlers;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Pagination;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Orders;

public sealed class FetchOrdersPageHandlerTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_WithNoAfter_ShouldReturnFirstPage()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);
        await CreateOrderAsync(articleId, 3);
        await CreateOrderAsync(articleId, 4);

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.True(page.HasNext);
        Assert.NotNull(page.Next);

        // Verify orders are returned in ascending order by ID
        Assert.True(string.Compare(page.Items[0].Id.Value, page.Items[1].Id.Value, StringComparison.Ordinal) < 0);
    }

    [Fact]
    public async Task FetchAsync_WithAfter_ShouldReturnNextPage()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);
        await CreateOrderAsync(articleId, 3);
        await CreateOrderAsync(articleId, 4);

        var page1Result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(page1Result.Ok);

        var firstPage = page1Result.Value;

        Assert.Equal(2, firstPage.Size);
        Assert.True(firstPage.HasNext);
        Assert.NotNull(firstPage.Next);

        var page2Result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = firstPage.Next,
                Size = 2
            })
        );

        Assert.True(page2Result.Ok);

        var secondPage = page2Result.Value;

        Assert.Equal(2, secondPage.Size);
        Assert.False(secondPage.HasNext);
        Assert.Null(secondPage.Next);
    }

    [Fact]
    public async Task FetchAsync_WithSizeLargerThanAvailable_ShouldReturnAllOrders()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 10
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_WithZeroSize_ShouldReturnEmptyPage()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 0
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_WithNoOrders_ShouldReturnEmptyPage()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 5
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnCorrectOrderData()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Test Article", 500);

        var orderId = await CreateOrderAsync(articleId, 3);

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(1, page.Size);
        Assert.Equal(orderId.Value, page.Items[0].Id.Value);
        Assert.Equal(Cents.From(1500), page.Items[0].Total);
    }

    [Fact]
    public async Task FetchAsync_WithExactPageSize_ShouldSetHasNextCorrectly()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);
        await CreateOrderAsync(articleId, 3);

        var firstPageResult = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(firstPageResult.Ok);

        var firstPage = firstPageResult.Value;

        Assert.Equal(2, firstPage.Size);
        Assert.True(firstPage.HasNext);
        Assert.NotNull(firstPage.Next);

        var secondPageResult = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = firstPage.Next,
                Size = 2
            })
        );

        Assert.True(secondPageResult.Ok);

        var secondPage = secondPageResult.Value;

        Assert.Equal(1, secondPage.Size);
        Assert.False(secondPage.HasNext);
        Assert.Null(secondPage.Next);
    }

    [Fact]
    public async Task FetchAsync_WithNullPageRequest_ShouldThrowArgumentNullException()
    {
        await PrepareEnvironmentAsync();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler => handler.ExecuteAsync(null!)
            );
        });
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnAccumulatedViewPlusOneMorePage()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        var order1Id = await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);
        await CreateOrderAsync(articleId, 3);

        var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
            handler.ExecuteAsync(new PageRequest
            {
                After = null,
                Until = order1Id,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.Equal(order1Id.Value, page.Items[0].Id.Value);
    }

    [Fact]
    public async Task FetchAsync_PaginatingThroughAllItems_ShouldNeverReturnDuplicates()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        await CreateOrderAsync(articleId, 1);
        await CreateOrderAsync(articleId, 2);
        await CreateOrderAsync(articleId, 3);
        await CreateOrderAsync(articleId, 4);
        await CreateOrderAsync(articleId, 5);

        var allIds = new List<string>();
        Identifier? cursor = null;

        for (var i = 0; i < 10; i++)
        {
            var result = await RunScoped<IFetchOrdersPageHandler, Result<Page<OrderListItem>>>(handler =>
                handler.ExecuteAsync(new PageRequest
                {
                    After = cursor,
                    Size = 2
                })
            );

            Assert.True(result.Ok);
            var page = result.Value;

            foreach (var order in page.Items)
            {
                allIds.Add(order.Id.Value);
            }

            if (!page.HasNext)
            {
                break;
            }

            cursor = page.Next;
        }

        Assert.Equal(5, allIds.Count);
        Assert.Equal(allIds.Distinct().Count(), allIds.Count);
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