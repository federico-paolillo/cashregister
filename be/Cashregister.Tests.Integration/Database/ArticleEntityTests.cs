using CashRegister.Database;
using CashRegister.Database.Entities;

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

        var wArticleEntity = new ArticleEntity
        {
            Id = "some-id", 
            Description = "Test Article", 
            Price = 1200
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
        Assert.Equal("some-id", rArticleEntity.Id);
    }
}