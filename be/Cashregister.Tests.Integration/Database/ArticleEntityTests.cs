using Cashregister.Database;
using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Database;

public sealed class ArticleEntityTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task Article_Entity_Is_Rw()
    {
        await PrepareEnvironmentAsync();

        ArticleEntity wArticleEntity = new()
        {
            Id = "some-id",
            Description = "Test Article",
            Price = 1200,
            PrintDetailReceipt = false,
            QuantityAvailable = -4,
            Retired = false
        };

        using var wScope = NewServiceScope();

        var wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        wDbContext.Articles.Add(wArticleEntity);

        await wDbContext.SaveChangesAsync();

        using var rScope = NewServiceScope();

        var rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rArticleEntity = await rDbContext.Articles.SingleOrDefaultAsync(a => a.Id == "some-id");

        Assert.NotNull(rArticleEntity);

        Assert.Equal("Test Article", rArticleEntity.Description);
        Assert.Equal(1200, rArticleEntity.Price);
        Assert.False(rArticleEntity.PrintDetailReceipt);
        Assert.Equal(-4, rArticleEntity.QuantityAvailable);
        Assert.Equal("some-id", rArticleEntity.Id);
    }
}