using Cashregister.Database;
using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;

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
            Price = 1200,
            Retired = false
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
                    Description = "some description",
                    Price = 1200,
                    Quantity = 10
                }
            ]
        };

        using var wScope = NewServiceScope();

        var wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        wDbContext.Articles.Add(wArticleEntity);
        wDbContext.Orders.Add(wOrderEntity);

        await wDbContext.SaveChangesAsync();

        using var rScope = NewServiceScope();

        var rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rOrderEntity = await rDbContext.Orders
            .Include(orderEntity => orderEntity.Items)
            .SingleOrDefaultAsync(a => a.Id == "some-id");

        Assert.NotNull(rOrderEntity);
        Assert.NotEqual(0L, rOrderEntity.RowId);
        Assert.Single(rOrderEntity.Items);

        Assert.Equal("some-order-item-id", rOrderEntity.Items[0].Id);
        Assert.Equal("some-article-id", rOrderEntity.Items[0].ArticleId);
        Assert.Equal("some-id", rOrderEntity.Items[0].OrderId);
        Assert.Equal("some description", rOrderEntity.Items[0].Description);
        Assert.Equal(1200, rOrderEntity.Items[0].Price);
        Assert.Equal(10U, rOrderEntity.Items[0].Quantity);
    }

    [Fact]
    public async Task Order_Entity_With_TotalOverride_Is_Rw()
    {
        await PrepareEnvironmentAsync();

        ArticleEntity wArticleEntity = new()
        {
            Id = "article-for-override",
            Description = "Override Article",
            Price = 500,
            Retired = false
        };

        OrderEntity wOrderEntity = new()
        {
            Id = "order-with-override",
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TotalOverride = 999,
            Items =
            [
                new OrderItemEntity
                {
                    Id = "item-for-override",
                    ArticleId = "article-for-override",
                    OrderId = "order-with-override",
                    Description = "Override Article",
                    Price = 500,
                    Quantity = 3
                }
            ]
        };

        using var wScope = NewServiceScope();
        var wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        wDbContext.Articles.Add(wArticleEntity);
        wDbContext.Orders.Add(wOrderEntity);
        await wDbContext.SaveChangesAsync();

        using var rScope = NewServiceScope();
        var rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rOrderEntity = await rDbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == "order-with-override");

        Assert.NotNull(rOrderEntity);
        Assert.Equal(999L, rOrderEntity.TotalOverride);
    }

    [Fact]
    public async Task Order_Entity_Without_TotalOverride_Has_Null()
    {
        await PrepareEnvironmentAsync();

        ArticleEntity wArticleEntity = new()
        {
            Id = "article-no-override",
            Description = "No Override Article",
            Price = 500,
            Retired = false
        };

        OrderEntity wOrderEntity = new()
        {
            Id = "order-no-override",
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Items =
            [
                new OrderItemEntity
                {
                    Id = "item-no-override",
                    ArticleId = "article-no-override",
                    OrderId = "order-no-override",
                    Description = "No Override Article",
                    Price = 500,
                    Quantity = 1
                }
            ]
        };

        using var wScope = NewServiceScope();
        var wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        wDbContext.Articles.Add(wArticleEntity);
        wDbContext.Orders.Add(wOrderEntity);
        await wDbContext.SaveChangesAsync();

        using var rScope = NewServiceScope();
        var rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rOrderEntity = await rDbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == "order-no-override");

        Assert.NotNull(rOrderEntity);
        Assert.Null(rOrderEntity.TotalOverride);
    }
}