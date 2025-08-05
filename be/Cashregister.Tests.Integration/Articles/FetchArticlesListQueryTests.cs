using System.Collections.Immutable;

using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Articles;

public sealed class FetchArticlesListQueryTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
  [Fact]
  public async Task FetchAsync_WithNoAfter_ShouldReturnFirstArticlesInOrder()
  {
    await PrepareEnvironmentAsync();

    // Create multiple articles with different descriptions
    var article1Id = await CreateArticleAsync("Article A", 100);
    var article2Id = await CreateArticleAsync("Article B", 200);
    var article3Id = await CreateArticleAsync("Article C", 300);

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(2)
    );

    Assert.Equal(2, result.Length);

    // Verify articles are returned in ascending order by ID (ULID)
    Assert.True(string.Compare(result[0].Id.Value, result[1].Id.Value, StringComparison.Ordinal) < 0);

    // Verify the first two articles are returned
    var expectedIds = new[] { article1Id, article2Id, article3Id }
        .OrderBy(id => id.Value)
        .Take(2)
        .ToArray();

    Assert.Equal(expectedIds[0].Value, result[0].Id.Value);
    Assert.Equal(expectedIds[1].Value, result[1].Id.Value);
  }

  [Fact]
  public async Task FetchAsync_WithAfter_ShouldReturnArticlesAfterSpecifiedId()
  {
    await PrepareEnvironmentAsync();

    // Create multiple articles
    var article1Id = await CreateArticleAsync("Article A", 100);
    var article2Id = await CreateArticleAsync("Article B", 200);
    var article3Id = await CreateArticleAsync("Article C", 300);
    var article4Id = await CreateArticleAsync("Article D", 400);

    // Get all articles ordered by ID to find the middle one
    var allArticles = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(10)
    );

    Assert.True(allArticles.Length >= 3, "Need at least 3 articles for this test");
    var sortedIds = allArticles.OrderBy(a => a.Id.Value).ToArray();
    var afterId = sortedIds[1].Id; // Use the second article as the "after" point

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(2, afterId)
    );

    Assert.Equal(2, result.Length);

    // Verify all returned articles have IDs greater than the "after" ID
    foreach (var article in result)
    {
      Assert.True(string.Compare(article.Id.Value, afterId.Value, StringComparison.Ordinal) > 0);
    }

    // Verify articles are returned in ascending order
    Assert.True(string.Compare(result[0].Id.Value, result[1].Id.Value, StringComparison.Ordinal) < 0);
  }

  [Fact]
  public async Task FetchAsync_WithAfterLastArticle_ShouldReturnEmptyArray()
  {
    await PrepareEnvironmentAsync();

    var article1Id = await CreateArticleAsync("Article A", 100);
    var article2Id = await CreateArticleAsync("Article B", 200);

    // Get the last article ID
    var allArticles = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(10)
    );

    Assert.True(allArticles.Length >= 2, "Need at least 2 articles for this test");
    var lastArticleId = allArticles.OrderBy(a => a.Id.Value).Last().Id;

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(5, lastArticleId)
    );

    Assert.Empty(result);
  }

  [Fact]
  public async Task FetchAsync_WithCountLargerThanAvailable_ShouldReturnAllAvailableArticles()
  {
    await PrepareEnvironmentAsync();

    var article1Id = await CreateArticleAsync("Article A", 100);
    var article2Id = await CreateArticleAsync("Article B", 200);

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(10)
    );

    Assert.Equal(2, result.Length);

    // Verify articles are returned in ascending order
    Assert.True(string.Compare(result[0].Id.Value, result[1].Id.Value, StringComparison.Ordinal) < 0);
  }

  [Fact]
  public async Task FetchAsync_WithZeroCount_ShouldReturnEmptyArray()
  {
    await PrepareEnvironmentAsync();

    await CreateArticleAsync("Article A", 100);
    await CreateArticleAsync("Article B", 200);

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(0)
    );

    Assert.Empty(result);
  }

  [Fact]
  public async Task FetchAsync_WithNoArticles_ShouldReturnEmptyArray()
  {
    await PrepareEnvironmentAsync();

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(5)
    );

    Assert.Empty(result);
  }

  [Fact]
  public async Task FetchAsync_ShouldReturnCorrectArticleData()
  {
    await PrepareEnvironmentAsync();

    var description = "Test Article Description";
    var price = Cents.From(1500L);

    var articleId = await CreateArticleAsync(description, price.Value);

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(1)
    );

    Assert.Single(result);
    Assert.Equal(articleId.Value, result[0].Id.Value);
    Assert.Equal(description, result[0].Description);
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