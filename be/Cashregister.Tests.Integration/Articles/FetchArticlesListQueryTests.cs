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
  public async Task FetchAsync_WithNext_ShouldReturnArticlesFromSpecifiedNext()
  {
    await PrepareEnvironmentAsync();

    _ = await CreateArticleAsync("Article A", 100);
    
    var article2Id = await CreateArticleAsync("Article B", 200);
    var article3Id = await CreateArticleAsync("Article C", 300);
    var article4Id = await CreateArticleAsync("Article D", 400);

    var result = await RunScoped<IFetchArticlesListQuery, ImmutableArray<ArticleListItem>>(
        fetcher => fetcher.FetchAsync(3, article2Id)
    );

    Assert.Equal(3, result.Length);

    Assert.Equal(article2Id.Value, result[0].Id.Value);
    Assert.Equal(article3Id.Value, result[1].Id.Value);
    Assert.Equal(article4Id.Value, result[2].Id.Value);
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