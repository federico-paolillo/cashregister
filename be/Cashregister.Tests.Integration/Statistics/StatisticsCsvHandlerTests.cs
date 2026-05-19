using System.Text;

using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Application.Statistics.Handlers;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Statistics;

public sealed class StatisticsCsvHandlerTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task WriteArticleStatisticsCsvHandler_WritesArticleStatisticsCsv()
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

        using MemoryStream stream = new();

        await RunScoped<IWriteArticleStatisticsCsvHandler, Unit>(async handler =>
        {
            await handler.ExecuteAsync(stream);
            return default;
        });

        var csv = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Contains("ArticleId,Description,SoldUnits,OrdersIncluded,Volume", csv);
        Assert.Contains($"{articleId.Value},Article A,2,1,2.00", csv);
        Assert.DoesNotContain("Total", csv);
    }

    [Fact]
    public async Task WriteOrderStatisticsCsvHandler_WritesOrderStatisticsCsv()
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

        using MemoryStream stream = new();

        await RunScoped<IWriteOrderStatisticsCsvHandler, Unit>(async handler =>
        {
            await handler.ExecuteAsync(stream);
            return default;
        });

        var csv = Encoding.UTF8.GetString(stream.ToArray());

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