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

    // Create multiple articles
    await CreateArticleAsync("Article A", 100);
    await CreateArticleAsync("Article B", 200);
    await CreateArticleAsync("Article C", 300);
    await CreateArticleAsync("Article D", 400);

    // Get first page to get the "after" cursor
    var firstPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
        tx => tx.ExecuteAsync(new ArticlesPageRequest
        {
          After = null,
          Size = 2
        })
    );

    Assert.True(firstPageResult.Ok);
    var firstPage = firstPageResult.Value;
    Assert.NotNull(firstPage.Next);

    // Get second page using the cursor
    var secondPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
        tx => tx.ExecuteAsync(new ArticlesPageRequest
        {
          After = firstPage.Next,
          Size = 2
        })
    );

    Assert.True(secondPageResult.Ok);
    var secondPage = secondPageResult.Value;

    Assert.Equal(1, secondPage.Size); // Only 1 article left after the cursor
    Assert.False(secondPage.HasNext); // No more articles after this
    Assert.Null(secondPage.Next);

    // Verify that second page articles have IDs greater than the cursor
    foreach (var article in secondPage.Articles)
    {
      Assert.True(string.Compare(article.Id.Value, firstPage.Next.Value, StringComparison.Ordinal) > 0);
    }
  }

  [Fact]
  public async Task FetchAsync_WithAfterLastArticle_ShouldReturnEmptyPage()
  {
    await PrepareEnvironmentAsync();

    await CreateArticleAsync("Article A", 100);
    await CreateArticleAsync("Article B", 200);

    // Get all articles to find the last one
    var allArticlesResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
        tx => tx.ExecuteAsync(new ArticlesPageRequest
        {
          After = null,
          Size = 10
        })
    );

    Assert.True(allArticlesResult.Ok);
    var allArticles = allArticlesResult.Value;
    var lastArticleId = allArticles.Articles.OrderBy(a => a.Id.Value).Last().Id;

    // Try to get page after the last article
    var result = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
        tx => tx.ExecuteAsync(new ArticlesPageRequest
        {
          After = lastArticleId,
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

    // Create exactly 3 articles
    await CreateArticleAsync("Article A", 100);
    await CreateArticleAsync("Article B", 200);
    await CreateArticleAsync("Article C", 300);

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

    // Get the next page
    var nextPageResult = await RunScoped<IFetchArticlesPageTransaction, Result<ArticlesPage>>(
        tx => tx.ExecuteAsync(new ArticlesPageRequest
        {
          After = page.Next,
          Size = 2
        })
    );

    Assert.True(nextPageResult.Ok);
    var nextPage = nextPageResult.Value;

    Assert.Equal(0, nextPage.Size); // No articles left after the cursor
    Assert.False(nextPage.HasNext);
    Assert.Null(nextPage.Next);
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