using Cashregister.Database;
using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Database;

public sealed class OrderEntityTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task Order_Entity_Is_Rw()
    {
        await PrepareEnvironmentAsync();

        ArticleEntity wArticleEntity = new()
        {
            Id = "some-article-id",
            Description = "Test Article",
            Price = 1200
        };

        OrderEntity wOrderEntity = new()
        {
            Id = "some-id",
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Items =
            [
                new OrderItemEntity
                {
                    Id = "some-order-item-id",
                    ArticleId = "some-article-id",
                    OrderId = "some-id",
                    Description = "Some description",
                    Price = 1200
                }
            ]
        };

        using IServiceScope wScope = NewServiceScope();

        ApplicationDbContext wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        wDbContext.Articles.Add(wArticleEntity);
        wDbContext.Orders.Add(wOrderEntity);

        await wDbContext.SaveChangesAsync();

        using IServiceScope rScope = NewServiceScope();

        ApplicationDbContext rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        OrderEntity? rOrderEntity = await rDbContext.Orders
            .Include(orderEntity => orderEntity.Items)
            .SingleOrDefaultAsync(a => a.Id == "some-id");

        Assert.NotNull(rOrderEntity);

        Assert.Single(rOrderEntity.Items);

        Assert.NotEqual(0L, rOrderEntity.RowId);
    }
}