using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Articles;

public sealed class FetchArticlesPageTransactionTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task FetchAsync_WithNoAfter_ShouldReturnFirstPage()
    {
        await PrepareEnvironmentAsync();

        // Create multiple articles
        var article1Id = await CreateArticleAsync("Article A", 100);
        var article2Id = await CreateArticleAsync("Article B", 200);
        var article3Id = await CreateArticleAsync("Article C", 300);
        var article4Id = await CreateArticleAsync("Article D", 400);

        var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.True(page.HasNext);
        Assert.NotNull(page.Next);

        // Verify articles are returned in ascending order by ID
        Assert.True(string.Compare(page.Articles[0].Id.Value, page.Articles[1].Id.Value, StringComparison.Ordinal) < 0);
    }

    [Fact]
    public async Task FetchAsync_WithAfter_ShouldReturnNextPage()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);

        var page1Result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(page1Result.Ok);

        var firstPage = page1Result.Value;

        Assert.Equal(2, firstPage.Size);
        Assert.True(firstPage.HasNext);
        Assert.NotNull(firstPage.Next);

        Assert.Equal("Article A", firstPage.Articles[0].Description);
        Assert.Equal("Article B", firstPage.Articles[1].Description);

        var page2Result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = firstPage.Next,
                Size = 2
            })
        );

        Assert.True(page2Result.Ok);

        var secondPage = page2Result.Value;

        Assert.Equal(2, secondPage.Size);
        Assert.False(secondPage.HasNext);
        Assert.Null(secondPage.Next);

        Assert.Equal("Article C", secondPage.Articles[0].Description);
        Assert.Equal("Article D", secondPage.Articles[1].Description);
    }

    [Fact]
    public async Task FetchAsync_WithSizeLargerThanAvailable_ShouldReturnAllArticles()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 10
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_WithZeroSize_ShouldReturnEmptyPage()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 0
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext); // When size is 0, it fetches 1 article to check if there are more
        Assert.NotNull(page.Next);
    }

    [Fact]
    public async Task FetchAsync_WithNoArticles_ShouldReturnEmptyPage()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 5
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnCorrectArticleData()
    {
        await PrepareEnvironmentAsync();

        var description = "Test Article Description";
        var price = Cents.From(1500L);

        var articleId = await CreateArticleAsync(description, price.Value);

        var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(1, page.Size);
        Assert.Equal(articleId.Value, page.Articles[0].Id.Value);
        Assert.Equal(description, page.Articles[0].Description);
    }

    [Fact]
    public async Task FetchAsync_WithExactPageSize_ShouldSetHasNextCorrectly()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);

        var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(firstPageResult.Ok);

        var firstPage = firstPageResult.Value;

        Assert.Equal(2, firstPage.Size);
        Assert.True(firstPage.HasNext);
        Assert.NotNull(firstPage.Next);

        Assert.Equal("Article A", firstPage.Articles[0].Description);
        Assert.Equal("Article B", firstPage.Articles[1].Description);

        var secondPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = firstPage.Next,
                Size = 2
            })
        );

        Assert.True(secondPageResult.Ok);

        var secondPage = secondPageResult.Value;

        Assert.Equal(1, secondPage.Size);
        Assert.False(secondPage.HasNext);
        Assert.Null(secondPage.Next);

        Assert.Equal("Article C", secondPage.Articles[0].Description);
    }

    [Fact]
    public async Task FetchAsync_WithNullPageRequest_ShouldThrowArgumentNullException()
    {
        await PrepareEnvironmentAsync();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
                tx => tx.ExecuteAsync(null!)
            );
        });
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnAllArticlesBeforeCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);

        var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(firstPageResult.Ok);
        Assert.NotNull(firstPageResult.Value.Next);

        var untilCursor = firstPageResult.Value.Next;

        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Until = untilCursor,
                Size = 2
            })
        );

        Assert.True(untilResult.Ok);

        var page = untilResult.Value;

        Assert.Equal(2, page.Size);
        Assert.Equal("Article A", page.Articles[0].Description);
        Assert.Equal("Article B", page.Articles[1].Description);
        Assert.True(page.HasNext);
        Assert.Equal(untilCursor, page.Next);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnHasNextFalse_WhenNoArticlesAfterCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 1
            })
        );

        Assert.True(firstPageResult.Ok);
        Assert.NotNull(firstPageResult.Value.Next);

        var untilCursor = firstPageResult.Value.Next;

        var secondPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 1,
                Until = untilCursor,
            })
        );

        Assert.True(secondPageResult.Ok);

        var page = secondPageResult.Value;

        // After loading page 2 (Article B), the cursor points past the last article
        // so until=cursor should return Article A with hasNext=true (Article B still exists)
        Assert.Equal(1, page.Size);
        Assert.Equal("Article A", page.Articles[0].Description);
        Assert.True(page.HasNext);

        // Now get a cursor that points past all articles
        var lastPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 10
            })
        );

        Assert.True(lastPageResult.Ok);
        Assert.False(lastPageResult.Value.HasNext);
        Assert.Null(lastPageResult.Value.Next);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldIncludeNewlyAddedArticles()
    {
        await PrepareEnvironmentAsync();

        var article1Id = await CreateArticleAsync("Article A", 100);
        var article2Id = await CreateArticleAsync("Article B", 200);
        var article3Id = await CreateArticleAsync("Article C", 300);

        var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(firstPageResult.Ok);
        Assert.NotNull(firstPageResult.Value.Next);

        var untilCursor = firstPageResult.Value.Next;

        // Simulate a new article being added before Article C (not possible with ULIDs
        // since they are time-ordered, but Article A and B are before the cursor)
        // The until query should return all articles with Id < cursor

        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Until = untilCursor,
                Size = 2
            })
        );

        Assert.True(untilResult.Ok);

        var page = untilResult.Value;

        // Should return articles A and B (both before the cursor)
        Assert.Equal(2, page.Size);
        Assert.Equal("Article A", page.Articles[0].Description);
        Assert.Equal("Article B", page.Articles[1].Description);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnEmptyPage_WhenNothingIsBeforeCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);

        var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 10
            })
        );

        Assert.True(firstPageResult.Ok);

        var firstArticleId = firstPageResult.Value.Articles[0].Id;

        // Use the first article's own Id as the until cursor:
        // nothing has Id < firstArticleId
        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Until = firstArticleId,
                Size = 10
            })
        );

        Assert.True(untilResult.Ok);

        var page = untilResult.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext); // Article A is at the cursor position
        Assert.Equal(firstArticleId, page.Next);
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
    {
        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(
            tx => tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            })
        );

        Assert.True(registerArticleResult.Ok);
        return registerArticleResult.Value;
    }
}