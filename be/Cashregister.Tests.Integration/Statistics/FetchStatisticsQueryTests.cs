using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Statistics;

public sealed class FetchStatisticsQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_ReturnsZeroStatistics_WhenDatabaseIsEmpty()
    {
        await PrepareEnvironmentAsync();

        var statistics = await RunScoped<IFetchStatisticsQuery, OrderStatistics>(query =>
            query.FetchAsync()
        );

        Assert.Empty(statistics.Articles.Items);
        Assert.Equal(0, statistics.Articles.Totals.SoldUnits);
        Assert.Equal(0, statistics.Articles.Totals.OrdersIncluded);
        Assert.Equal(0, statistics.Articles.Totals.VolumeInCents);
        Assert.Equal(0, statistics.Orders.OrderCount);
        Assert.Equal(0, statistics.Orders.NominalVolumeInCents);
        Assert.Equal(0, statistics.Orders.RealVolumeInCents);
        Assert.Equal(0, statistics.Orders.DeltaInCents);
        Assert.Equal(statistics.Orders, statistics.OrdersTotals);
    }

    [Fact]
    public async Task FetchAsync_ReturnsActiveArticlesWithZeroSales()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Unsold article", 100L);

        var statistics = await RunScoped<IFetchStatisticsQuery, OrderStatistics>(query =>
            query.FetchAsync()
        );

        var articleStatistics = Assert.Single(statistics.Articles.Items);
        Assert.Equal(articleId, articleStatistics.ArticleId);
        Assert.Equal("Unsold article", articleStatistics.Description);
        Assert.Equal(0, articleStatistics.SoldUnits);
        Assert.Equal(0, articleStatistics.OrdersIncluded);
        Assert.Equal(0, articleStatistics.VolumeInCents);
        Assert.Equal(0, statistics.Articles.Totals.SoldUnits);
        Assert.Equal(0, statistics.Articles.Totals.OrdersIncluded);
        Assert.Equal(0, statistics.Articles.Totals.VolumeInCents);
    }

    [Fact]
    public async Task FetchAsync_ReturnsActiveArticleRowsAndAllHistoricalOrderTotals()
    {
        await PrepareEnvironmentAsync();

        var articleA = await CreateArticleAsync("Article A", 100L);
        var articleB = await CreateArticleAsync("Article B", 250L);
        var retiredArticle = await CreateArticleAsync("Retired article", 1000L);

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = articleA,
                    Quantity = 2
                },
                new OrderRequestItem
                {
                    Article = articleB,
                    Quantity = 1
                }
            ]
        );

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = articleA,
                    Quantity = 3
                }
            ],
            Cents.From(250L)
        );

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = retiredArticle,
                    Quantity = 2
                }
            ],
            Cents.From(2500L)
        );

        await RetireArticleAsync(retiredArticle);

        var statistics = await RunScoped<IFetchStatisticsQuery, OrderStatistics>(query =>
            query.FetchAsync()
        );

        Assert.Equal(2, statistics.Articles.Items.Length);

        var articleAStatistics = statistics.Articles.Items.Single(x => x.ArticleId == articleA);
        Assert.Equal("Article A", articleAStatistics.Description);
        Assert.Equal(5, articleAStatistics.SoldUnits);
        Assert.Equal(2, articleAStatistics.OrdersIncluded);
        Assert.Equal(500L, articleAStatistics.VolumeInCents);

        var articleBStatistics = statistics.Articles.Items.Single(x => x.ArticleId == articleB);
        Assert.Equal("Article B", articleBStatistics.Description);
        Assert.Equal(1, articleBStatistics.SoldUnits);
        Assert.Equal(1, articleBStatistics.OrdersIncluded);
        Assert.Equal(250L, articleBStatistics.VolumeInCents);

        Assert.DoesNotContain(statistics.Articles.Items, x => x.ArticleId == retiredArticle);
        Assert.Equal(6, statistics.Articles.Totals.SoldUnits);
        Assert.Equal(3, statistics.Articles.Totals.OrdersIncluded);
        Assert.Equal(750L, statistics.Articles.Totals.VolumeInCents);

        Assert.Equal(3, statistics.Orders.OrderCount);
        Assert.Equal(2750L, statistics.Orders.NominalVolumeInCents);
        Assert.Equal(3200L, statistics.Orders.RealVolumeInCents);
        Assert.Equal(450L, statistics.Orders.DeltaInCents);
        Assert.Equal(statistics.Orders, statistics.OrdersTotals);
    }

    [Fact]
    public async Task FetchAsync_UsesHistoricalOrderItemPrice_WhenArticlePriceChanges()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Changing article", 100L);

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = articleId,
                    Quantity = 2
                }
            ]
        );

        await ChangeArticleAsync(articleId, "Changing article", 500L);

        var statistics = await RunScoped<IFetchStatisticsQuery, OrderStatistics>(query =>
            query.FetchAsync()
        );

        var articleStatistics = Assert.Single(statistics.Articles.Items);
        Assert.Equal(2, articleStatistics.SoldUnits);
        Assert.Equal(1, articleStatistics.OrdersIncluded);
        Assert.Equal(200L, articleStatistics.VolumeInCents);
        Assert.Equal(200L, statistics.Articles.Totals.VolumeInCents);
        Assert.Equal(200L, statistics.Orders.NominalVolumeInCents);
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
    {
        var result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            })
        );

        Assert.True(result.Ok);
        return result.Value;
    }

    private async Task ChangeArticleAsync(Identifier articleId, string description, long priceInCents)
    {
        var result = await RunScoped<IChangeArticleTransaction, Result<Unit>>(tx =>
            tx.ExecuteAsync(new ArticleChange
            {
                Id = articleId,
                Description = description,
                Price = Cents.From(priceInCents)
            })
        );

        Assert.True(result.Ok);
    }

    private async Task RetireArticleAsync(Identifier articleId)
    {
        var result = await RunScoped<IRetireArticleTransaction, Result<Unit>>(tx =>
            tx.ExecuteAsync(articleId)
        );

        Assert.True(result.Ok);
    }

    private async Task CreateOrderAsync(
        IReadOnlyCollection<OrderRequestItem> items,
        Cents? totalOverride = null
    )
    {
        var result = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new OrderRequest
            {
                Items = [.. items],
                TotalOverride = totalOverride
            })
        );

        Assert.True(result.Ok);
    }
}