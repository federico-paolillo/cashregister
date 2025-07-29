using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Orders.Queries;
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

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article",
                Price = Cents.From(1L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        var articleId = registerArticleResult.Value;

        var placeOrderResult = await RunScoped<IPlaceOrderTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new OrderRequest
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

        var orderSummary = await RunScoped<IFetchOrderSummaryQuery, OrderSummary?>(
            q => q.FetchAsync(orderId)
        );

        Assert.NotNull(orderSummary);
        Assert.Equal(Cents.From(21L), orderSummary.Total);
    }
}