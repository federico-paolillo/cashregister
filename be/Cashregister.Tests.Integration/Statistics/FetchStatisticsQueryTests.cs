using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Statistics.Data;
using Cashregister.Application.Statistics.Models.Output;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

using StatisticsOutput = Cashregister.Application.Statistics.Models.Output.StatisticsReport;

namespace Cashregister.Tests.Integration.Statistics;

public sealed class FetchStatisticsQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_ReturnsZeroStatistics_WhenDatabaseIsEmpty()
    {
        await PrepareEnvironmentAsync();

        var statistics = await RunScoped<IFetchStatisticsQuery, StatisticsOutput>(query =>
            query.FetchAsync()
        );

        Assert.Empty(statistics.Articles);
        Assert.Empty(statistics.Orders);
        Assert.Empty(statistics.SalesRows);
        Assert.Equal(0, statistics.Summary.OrderCount);
        Assert.Equal(0, statistics.Summary.ProducedArticles);
        Assert.Equal(0, statistics.Summary.ExpectedVolumeInCents);
        Assert.Equal(0, statistics.Summary.RealVolumeInCents);
        Assert.Equal(0, statistics.Summary.DeltaInCents);
    }

    [Fact]
    public async Task FetchAsync_ExcludesUnsoldArticles()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Unsold article", 100L);

        var statistics = await RunScoped<IFetchStatisticsQuery, StatisticsOutput>(query =>
            query.FetchAsync()
        );

        Assert.Empty(statistics.Articles);
    }

    [Fact]
    public async Task FetchAsync_ReturnsSoldArticleInventoryAndOrderRows()
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

        var statistics = await RunScoped<IFetchStatisticsQuery, StatisticsOutput>(query =>
            query.FetchAsync()
        );

        Assert.Equal(3, statistics.Articles.Length);

        var articleAStatistics = statistics.Articles.Single(x => x.ArticleId == articleA);
        Assert.Equal("Article A", articleAStatistics.Description);
        Assert.False(articleAStatistics.Retired);
        Assert.Equal(5, articleAStatistics.SoldUnits);

        var articleBStatistics = statistics.Articles.Single(x => x.ArticleId == articleB);
        Assert.Equal("Article B", articleBStatistics.Description);
        Assert.False(articleBStatistics.Retired);
        Assert.Equal(1, articleBStatistics.SoldUnits);

        var retiredArticleStatistics = statistics.Articles.Single(x => x.ArticleId == retiredArticle);
        Assert.Equal("Retired article", retiredArticleStatistics.Description);
        Assert.True(retiredArticleStatistics.Retired);
        Assert.Equal(2, retiredArticleStatistics.SoldUnits);

        Assert.Equal(3, statistics.Orders.Length);

        Assert.Collection(
            statistics.Orders,
            first =>
            {
                Assert.Equal(3, first.ProducedArticles);
                Assert.Equal(450L, first.ExpectedVolumeInCents);
                Assert.Equal(450L, first.RealVolumeInCents);
                Assert.Equal(0L, first.DeltaInCents);
                Assert.False(first.HasOverride);
            },
            second =>
            {
                Assert.Equal(3, second.ProducedArticles);
                Assert.Equal(300L, second.ExpectedVolumeInCents);
                Assert.Equal(250L, second.RealVolumeInCents);
                Assert.Equal(-50L, second.DeltaInCents);
                Assert.True(second.HasOverride);
            },
            third =>
            {
                Assert.Equal(2, third.ProducedArticles);
                Assert.Equal(2000L, third.ExpectedVolumeInCents);
                Assert.Equal(2500L, third.RealVolumeInCents);
                Assert.Equal(500L, third.DeltaInCents);
                Assert.True(third.HasOverride);
            }
        );

        Assert.Equal(3, statistics.Summary.OrderCount);
        Assert.Equal(8, statistics.Summary.ProducedArticles);
        Assert.Equal(2750L, statistics.Summary.ExpectedVolumeInCents);
        Assert.Equal(3200L, statistics.Summary.RealVolumeInCents);
        Assert.Equal(450L, statistics.Summary.DeltaInCents);
        Assert.Equal(4, statistics.SalesRows.Length);
    }

    [Fact]
    public async Task FetchAsync_UsesHistoricalOrderItemPriceAndCurrentArticleDescription()
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

        await ChangeArticleAsync(articleId, "Renamed article", 500L);

        var statistics = await RunScoped<IFetchStatisticsQuery, StatisticsOutput>(query =>
            query.FetchAsync()
        );

        var articleStatistics = Assert.Single(statistics.Articles);
        Assert.Equal("Renamed article", articleStatistics.Description);
        Assert.Equal(2, articleStatistics.SoldUnits);

        var orderStatistics = Assert.Single(statistics.Orders);
        Assert.Equal(200L, orderStatistics.ExpectedVolumeInCents);
        Assert.Equal(200L, orderStatistics.RealVolumeInCents);

        var salesRow = Assert.Single(statistics.SalesRows);
        Assert.Equal("Renamed article", salesRow.CurrentArticleDescription);
        Assert.Equal("Changing article", salesRow.SoldDescription);
        Assert.Equal(100L, salesRow.UnitPriceInCents);
        Assert.Equal(2, salesRow.Quantity);
        Assert.Null(salesRow.OrderTotalOverrideInCents);
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