using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Articles.Handlers;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Pagination;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Articles;

public sealed class FetchArticlesPageHandlerTests(
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

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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
        Assert.True(string.Compare(page.Items[0].Id.Value, page.Items[1].Id.Value, StringComparison.Ordinal) < 0);
    }

    [Fact]
    public async Task FetchAsync_WithAfter_ShouldReturnNextPage()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);

        var page1Result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        Assert.Equal("Article A", firstPage.Items[0].Description);
        Assert.Equal("Article B", firstPage.Items[1].Description);

        var page2Result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        Assert.Equal("Article C", secondPage.Items[0].Description);
        Assert.Equal("Article D", secondPage.Items[1].Description);
    }

    [Fact]
    public async Task FetchAsync_WithSizeLargerThanAvailable_ShouldReturnAllArticles()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 0
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext); // When size is 0, it fetches 1 article to check if there are more
        Assert.Null(page.Next); // No cursor available from an empty page
    }

    [Fact]
    public async Task FetchAsync_WithNoArticles_ShouldReturnEmptyPage()
    {
        await PrepareEnvironmentAsync();

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(1, page.Size);
        Assert.Equal(articleId.Value, page.Items[0].Id.Value);
        Assert.Equal(description, page.Items[0].Description);
    }

    [Fact]
    public async Task FetchAsync_WithExactPageSize_ShouldSetHasNextCorrectly()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);

        var firstPageResult = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        Assert.Equal("Article A", firstPage.Items[0].Description);
        Assert.Equal("Article B", firstPage.Items[1].Description);

        var secondPageResult = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
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

        Assert.Equal("Article C", secondPage.Items[0].Description);
    }

    [Fact]
    public async Task FetchAsync_WithNullPageRequest_ShouldThrowArgumentNullException()
    {
        await PrepareEnvironmentAsync();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
                tx => tx.ExecuteAsync(null!)
            );
        });
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReturnAccumulatedViewPlusOneMorePage()
    {
        await PrepareEnvironmentAsync();

        // With pageSize=1: page 1 returns [A], next=A_id (last item of current page).
        // Submitting until=A_id should extend the view to [A, B].
        var articleAId = await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Until = articleAId,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.Equal("Article A", page.Items[0].Description);
        Assert.Equal("Article B", page.Items[1].Description);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldSetNextCursorToLastItemOfNewPage()
    {
        await PrepareEnvironmentAsync();

        var articleAId = await CreateArticleAsync("Article A", 100);
        var articleBId = await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);

        // until=A_id with pageSize=1: historical=[A], next page=[B], lookahead hits C
        // Next cursor should be B (last item of the new page), not C
        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Until = articleAId,
                Size = 1
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.True(page.HasNext);
        Assert.NotNull(page.Next);
        Assert.Equal(articleBId.Value, page.Next.Value);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldReportHasNextFalse_WhenNothingExistsAtOrAfterCursor()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);

        // Identifier.New() generates a ULID after all existing ones, so nothing in the DB
        // has an ID >= this cursor.
        var beyondAllCursor = Identifier.New();

        var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Until = beyondAllCursor,
                Size = 50
            })
        );

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchAsync_PaginatingThroughAllItems_ShouldNeverReturnDuplicates()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);
        await CreateArticleAsync("Article E", 500);

        var allDescriptions = new List<string>();
        Identifier? cursor = null;

        // Page through all articles with pageSize=2
        for (var i = 0; i < 10; i++) // safety limit
        {
            var result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
                tx => tx.ExecuteAsync(new PageRequest
                {
                    After = cursor,
                    Size = 2
                })
            );

            Assert.True(result.Ok);
            var page = result.Value;

            foreach (var article in page.Items)
            {
                allDescriptions.Add(article.Description);
            }

            if (!page.HasNext)
                break;

            cursor = page.Next;
        }

        Assert.Equal(5, allDescriptions.Count);
        Assert.Equal(allDescriptions.Distinct().Count(), allDescriptions.Count);

        Assert.Equal("Article A", allDescriptions[0]);
        Assert.Equal("Article B", allDescriptions[1]);
        Assert.Equal("Article C", allDescriptions[2]);
        Assert.Equal("Article D", allDescriptions[3]);
        Assert.Equal("Article E", allDescriptions[4]);
    }

    [Fact]
    public async Task FetchAsync_WithUntil_ShouldNotOverlapWithPreviousPage()
    {
        await PrepareEnvironmentAsync();

        await CreateArticleAsync("Article A", 100);
        await CreateArticleAsync("Article B", 200);
        await CreateArticleAsync("Article C", 300);
        await CreateArticleAsync("Article D", 400);

        // Get page 1 with pageSize=2
        var page1Result = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Size = 2
            })
        );

        Assert.True(page1Result.Ok);
        var page1 = page1Result.Value;

        Assert.Equal(2, page1.Size);
        Assert.True(page1.HasNext);
        Assert.NotNull(page1.Next);

        // Use until with the Next cursor to get accumulated view
        var untilResult = await RunScoped<IFetchArticlesPageHandler, Result<Page<ArticleListItem>>>(
            tx => tx.ExecuteAsync(new PageRequest
            {
                After = null,
                Until = page1.Next,
                Size = 2
            })
        );

        Assert.True(untilResult.Ok);
        var untilPage = untilResult.Value;

        // Should contain the historical items [A, B] plus the next page [C, D]
        Assert.Equal(4, untilPage.Size);
        Assert.Equal("Article A", untilPage.Items[0].Description);
        Assert.Equal("Article B", untilPage.Items[1].Description);
        Assert.Equal("Article C", untilPage.Items[2].Description);
        Assert.Equal("Article D", untilPage.Items[3].Description);

        Assert.False(untilPage.HasNext);
        Assert.Null(untilPage.Next);
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