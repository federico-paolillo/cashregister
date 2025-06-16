using CashRegister.Application.Orders.Models.Input;
using CashRegister.Application.Orders.Queries;
using CashRegister.Application.Orders.Transactions;
using CashRegister.Database;
using CashRegister.Database.Entities;
using CashRegister.Domain;

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

        var articleId = "bla-bla";

        var testArticle = new ArticleEntity
        {
            Id = articleId,
            Description = "Some test article",
            Price = 100L
        };
        
        using var setupScope = NewServiceScope();

        var dbCtx = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbCtx.Articles.Add(testArticle);
        
        await dbCtx.SaveChangesAsync();

        using var wScope = NewServiceScope();

        var tx = wScope.ServiceProvider.GetRequiredService<IPlaceOrderTransaction>();
        
        var orderRequest = new OrderRequest
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

        var orderId = await tx.PlaceOrderAsync(orderRequest);
        
        Assert.NotNull(orderId);
        
        using var rScope = NewServiceScope();

        var orderSummaryQuery = rScope.ServiceProvider.GetRequiredService<IFetchOrderSummaryQuery>();

        var orderSummary = await orderSummaryQuery.FetchAsync(orderId);
        
        Assert.NotNull(orderSummary);
        Assert.Equal(orderId, orderSummary.Id);
    }
}