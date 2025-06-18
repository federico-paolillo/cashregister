using Cashregister.Database;
using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            Price = 1200
        };

        using IServiceScope wScope = NewServiceScope();

        ApplicationDbContext wDbContext = wScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        wDbContext.Articles.Add(wArticleEntity);

        await wDbContext.SaveChangesAsync();

        using IServiceScope rScope = NewServiceScope();

        ApplicationDbContext rDbContext = rScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        ArticleEntity? rArticleEntity = await rDbContext.Articles.SingleOrDefaultAsync(a => a.Id == "some-id");

        Assert.NotNull(rArticleEntity);

        Assert.Equal("Test Article", rArticleEntity.Description);
        Assert.Equal(1200, rArticleEntity.Price);
        Assert.Equal("some-id", rArticleEntity.Id);
    }
}