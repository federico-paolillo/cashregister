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
        var articleStatistics = Assert.Single(statistics.Articles.Items);
        Assert.Equal(articleId.Value, articleStatistics.ArticleId);
        Assert.Equal("Article A", articleStatistics.Description);
        Assert.Equal(2, articleStatistics.SoldUnits);
        Assert.Equal(1, articleStatistics.OrdersIncluded);
        Assert.Equal(200L, articleStatistics.VolumeInCents);
        Assert.Equal(2, statistics.Articles.Totals.SoldUnits);
        Assert.Equal(1, statistics.Articles.Totals.OrdersIncluded);
        Assert.Equal(200L, statistics.Articles.Totals.VolumeInCents);
        Assert.Equal(1, statistics.Orders.OrderCount);
        Assert.Equal(200L, statistics.Orders.NominalVolumeInCents);
        Assert.Equal(150L, statistics.Orders.RealVolumeInCents);
        Assert.Equal(-50L, statistics.Orders.DeltaInCents);
        Assert.Equal(statistics.Orders, statistics.OrdersTotals);
    }

    [Fact]
    public async Task GetArticleStatisticsCsv_ReturnsCsvAttachment()
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
            ]
        );

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/statistics/articles.csv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("statistics-articles.csv", response.Content.Headers.ContentDisposition?.FileName);

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains("ArticleId,Description,SoldUnits,OrdersIncluded,Volume", csv);
        Assert.Contains($"{articleId.Value},Article A,2,1,2.00", csv);
        Assert.DoesNotContain("Total", csv);
    }

    [Fact]
    public async Task GetOrderStatisticsCsv_ReturnsCsvAttachment()
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

        var response = await httpClient.GetAsync("/statistics/orders.csv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("statistics-orders.csv", response.Content.Headers.ContentDisposition?.FileName);

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains("OrderCount,NominalVolume,RealVolume,Delta", csv);
        Assert.Contains("1,2.00,1.50,-0.50", csv);
        Assert.DoesNotContain("Row", csv);
        Assert.DoesNotContain("Orders", csv);
        Assert.DoesNotContain("Total", csv);
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