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
    public async Task WriteSalesStatisticsCsvHandler_WritesRawSalesStatisticsCsv()
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

        await RunScoped<IWriteSalesStatisticsCsvHandler, Unit>(async handler =>
        {
            await handler.ExecuteAsync(stream);
            return default;
        });

        var csv = Encoding.UTF8.GetString(stream.ToArray());

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