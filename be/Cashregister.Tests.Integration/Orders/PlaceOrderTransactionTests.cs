using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

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

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article",
                Price = Cents.From(5L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        var articleId = registerArticleResult.Value;

        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new OrderRequest
                {
                    Items =
                    [
                        new OrderRequestItem
                        {
                            Article = articleId,
                            Quantity = 21
                        }
                    ]
                }
            )
        );

        Assert.True(placeOrderResult.Ok);

        var orderId = placeOrderResult.Value;

        var order = await RunScoped<IFetchOrderQuery, Order?>(q => q.FetchAsync(orderId)
        );

        Assert.NotNull(order);
        Assert.Equal(Cents.From(105L), order.Total());
        Assert.Single(order.Items);
        Assert.Equal("Some test article", order.Items[0].Description);
        Assert.Equal(21u, order.Items[0].Quantity);
    }

    [Fact]
    public async Task PlaceOrderTransaction_WithTotalOverride_ShouldApplyOverride()
    {
        await PrepareEnvironmentAsync();

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Override article",
                Price = Cents.From(100L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        var articleId = registerArticleResult.Value;

        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new OrderRequest
                {
                    Items =
                    [
                        new OrderRequestItem
                        {
                            Article = articleId,
                            Quantity = 5
                        }
                    ],
                    TotalOverride = Cents.From(350L)
                }
            )
        );

        Assert.True(placeOrderResult.Ok);

        var orderId = placeOrderResult.Value;

        var order = await RunScoped<IFetchOrderQuery, Order?>(q => q.FetchAsync(orderId));

        Assert.NotNull(order);
        Assert.Equal(Cents.From(350L), order.Total());
        Assert.Equal(Cents.From(350L), order.TotalOverride);
    }
}