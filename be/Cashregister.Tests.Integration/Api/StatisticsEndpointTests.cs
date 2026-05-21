using System.Net;

using Cashregister.Api.Statistics.Models;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class StatisticsEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task GetStatistics_ReturnsStatistics()
    {
        await PrepareEnvironmentAsync();
        var articleId = await CreateArticleAsync("Article A", 100L);

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = articleId,
                    Quantity = 2
                }
            ],
            Cents.From(150L)
        );

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/statistics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var statistics = await response.Content.ReadFromJsonAsync<StatisticsDto>();

        Assert.NotNull(statistics);
        var articleStatistics = Assert.Single(statistics.Articles);
        Assert.Equal(articleId.Value, articleStatistics.ArticleId);
        Assert.Equal("Article A", articleStatistics.Description);
        Assert.False(articleStatistics.Retired);
        Assert.Equal(2, articleStatistics.SoldUnits);

        var orderStatistics = Assert.Single(statistics.Orders);
        Assert.Equal(2, orderStatistics.ProducedArticles);
        Assert.Equal(200L, orderStatistics.ExpectedVolumeInCents);
        Assert.Equal(150L, orderStatistics.RealVolumeInCents);
        Assert.Equal(-50L, orderStatistics.DeltaInCents);
        Assert.True(orderStatistics.HasOverride);

        Assert.Equal(1, statistics.Summary.OrderCount);
        Assert.Equal(2, statistics.Summary.ProducedArticles);
        Assert.Equal(200L, statistics.Summary.ExpectedVolumeInCents);
        Assert.Equal(150L, statistics.Summary.RealVolumeInCents);
        Assert.Equal(-50L, statistics.Summary.DeltaInCents);
    }

    [Fact]
    public async Task GetSalesStatisticsCsv_ReturnsCsvAttachment()
    {
        await PrepareEnvironmentAsync();
        var articleId = await CreateArticleAsync("Article A", 100L);

        await CreateOrderAsync(
            [
                new OrderRequestItem
                {
                    Article = articleId,
                    Quantity = 2
                }
            ],
            Cents.From(150L)
        );

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/statistics/sales.csv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("statistics-sales.csv", response.Content.Headers.ContentDisposition?.FileName);

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "OrderId,OrderNumber,OrderDateUnixSeconds,OrderDateUtc,OrderItemId,ArticleId,CurrentArticleDescription,SoldDescription,ArticleRetired,UnitPriceInCents,Quantity,OrderTotalOverrideInCents",
            csv
        );
        Assert.Contains($",{articleId.Value},Article A,Article A,False,100,2,150", csv);
        Assert.DoesNotContain("ExpectedVolume", csv);
        Assert.DoesNotContain("RealVolume", csv);
        Assert.DoesNotContain("ProducedArticles", csv);
        Assert.DoesNotContain("OrderCount", csv);
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