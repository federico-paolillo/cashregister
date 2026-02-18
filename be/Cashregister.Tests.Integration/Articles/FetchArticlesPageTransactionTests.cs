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

        // Load page 1 to get the next cursor
        var page1Result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(page1Result.Ok);
        Assert.True(page1Result.Value.HasNext);
        Assert.NotNull(page1Result.Value.Next);

        // Load page 2 to get its next cursor
        var page2Result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = page1Result.Value.Next,
                Size = 2
            })
        );

        Assert.True(page2Result.Ok);

        // Now revalidate using until=page2.Next; should reconstruct page 1 + page 2
        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2,
                Until = page2Result.Value.Next
            })
        );

        Assert.True(untilResult.Ok);
        var page = untilResult.Value;

        // All four articles are returned because until points past article D
        Assert.Equal(4, page.Size);
        Assert.Equal("Article A", page.Articles[0].Description);
        Assert.Equal("Article B", page.Articles[1].Description);
        Assert.Equal("Article C", page.Articles[2].Description);
        Assert.Equal("Article D", page.Articles[3].Description);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldPreserveHasNextAndNextCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);

        // Load page 1 (size 2) to get the next cursor pointing at Article C
        var page1Result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(page1Result.Ok);
        Assert.True(page1Result.Value.HasNext);
        Assert.NotNull(page1Result.Value.Next);

        var untilCursor = page1Result.Value.Next;

        // until=cursor returns all articles before the cursor (A, B) and signals HasNext=true
        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 2,
                Until = untilCursor
            })
        );

        Assert.True(untilResult.Ok);
        var page = untilResult.Value;

        Assert.Equal(2, page.Size);
        Assert.Equal("Article A", page.Articles[0].Description);
        Assert.Equal("Article B", page.Articles[1].Description);

        // HasNext must be true so infinite scroll can continue
        Assert.True(page.HasNext);
        // Next must equal the until cursor so the client can resume forward pagination
        Assert.Equal(untilCursor!.Value, page.Next!.Value);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnEmptyList_WhenCursorIsBeforeAllArticles()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Article A", 100);

        // until is the very first article – nothing comes before it
        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 10,
                Until = articleId
            })
        );

        Assert.True(untilResult.Ok);
        var page = untilResult.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext);
        Assert.Equal(articleId.Value, page.Next!.Value);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnFalseHasNext_WhenNothingExistsAtOrAfterCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        // Fabricate a cursor that sorts after all existing articles (far-future ULID)
        var phantomCursor = Identifier.From("9" + new string('Z', 25));

        var untilResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
            tx => tx.ExecuteAsync(new ArticlesPageRequest
            {
                After = null,
                Size = 10,
                Until = phantomCursor
            })
        );

        Assert.True(untilResult.Ok);
        var page = untilResult.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
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