using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Queries;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Database;
using Cashregister.Database.Entities;
using Cashregister.Domain;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Orders;

public sealed class PlaceOrderTransactionTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task PlaceOrderTransaction_ShouldPlaceOrder()
    {
        await PrepareEnvironmentAsync();

        const string articleId = "bla-bla";

        ArticleEntity testArticle = new()
        {
            Id = articleId,
            Description = "Some test article",
            Price = 100L,
            Retired = false
        };

        using IServiceScope setupScope = NewServiceScope();

        ApplicationDbContext dbCtx = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbCtx.Articles.Add(testArticle);

        await dbCtx.SaveChangesAsync();

        using IServiceScope wScope = NewServiceScope();

        IPlaceOrderTransaction tx = wScope.ServiceProvider.GetRequiredService<IPlaceOrderTransaction>();

        OrderRequest orderRequest = new()
        {
            Items =
            [
                new OrderRequestItem
                {
                    Article = Identifier.From(articleId),
                    Quantity = 19
                }
            ]
        };

        var result = await tx.ExecuteAsync(orderRequest, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.NotNull(result.Value);

        var orderId = result.Value;

        using IServiceScope rScope = NewServiceScope();

        IFetchOrderSummaryQuery orderSummaryQuery =
            rScope.ServiceProvider.GetRequiredService<IFetchOrderSummaryQuery>();

        OrderSummary? orderSummary = await orderSummaryQuery.FetchAsync(orderId);

        Assert.NotNull(orderSummary);

        Assert.Equal(orderId, orderSummary.Id);
        Assert.Equal(20, orderSummary.Number.Value.Length);
    }
}