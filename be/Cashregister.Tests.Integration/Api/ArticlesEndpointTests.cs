using System.Net;

using Cashregister.Api.Articles.Models;
using Cashregister.Api.Commons.Models;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Domain;
using Cashregister.Factories;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class ArticlesEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task GetArticles_ReturnsEmptyList_WhenNoArticlesExist()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/articles");

        Assert.True(response.IsSuccessStatusCode);

        var articles = await response.Content.ReadFromJsonAsync<ArticlesPageDto>();

        Assert.NotNull(articles);
        Assert.Empty(articles.Items);
    }

    [Fact]
    public async Task GetArticles_ReturnsArticles_WhenArticlesExist()
    {
        await PrepareEnvironmentAsync();

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article",
                Price = Cents.From(1L)
            })
        );

        var registerArticleResult2 = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article 2",
                Price = Cents.From(1L)
            })
        );

        Assert.True(registerArticleResult.Ok);
        Assert.True(registerArticleResult2.Ok);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/articles");

        Assert.True(response.IsSuccessStatusCode);

        var articles = await response.Content.ReadFromJsonAsync<ArticlesPageDto>();

        Assert.NotNull(articles);

        Assert.Equal(2, articles.Items.Length);

        Assert.Equal(registerArticleResult.Value.Value, articles.Items[0].Id);
        Assert.Equal(registerArticleResult2.Value.Value, articles.Items[1].Id);
    }

    [Fact]
    public async Task GetArticles_ReturnsArticles_WhenArticlesExist_WithPagination()
    {
        await PrepareEnvironmentAsync();

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article",
                Price = Cents.From(1L)
            })
        );

        var registerArticleResult2 = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Some test article 2",
                Price = Cents.From(1L)
            })
        );

        Assert.True(registerArticleResult.Ok);
        Assert.True(registerArticleResult2.Ok);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/articles?pageSize=1");

        Assert.True(response.IsSuccessStatusCode);

        var articlesPage1 = await response.Content.ReadFromJsonAsync<ArticlesPageDto>();

        Assert.NotNull(articlesPage1);

        Assert.NotNull(articlesPage1.Next);
        Assert.True(articlesPage1.HasNext);

        Assert.Single(articlesPage1.Items);
        Assert.Equal(registerArticleResult.Value.Value, articlesPage1.Items[0].Id);

        response = await httpClient.GetAsync($"/articles?pageSize=1&after={articlesPage1.Next}");

        Assert.True(response.IsSuccessStatusCode);

        var articlesPage2 = await response.Content.ReadFromJsonAsync<ArticlesPageDto>();

        Assert.NotNull(articlesPage2);

        Assert.Null(articlesPage2.Next);
        Assert.False(articlesPage2.HasNext);

        Assert.Single(articlesPage2.Items);
        Assert.Equal(registerArticleResult2.Value.Value, articlesPage2.Items[0].Id);
    }

    [Fact]
    public async Task DeleteArticle_RetiresArticle_WhenArticleExists()
    {
        await PrepareEnvironmentAsync();

        var registerArticleResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article to delete",
                Price = Cents.From(100L)
            })
        );

        Assert.True(registerArticleResult.Ok);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.DeleteAsync($"/articles/{registerArticleResult.Value.Value}");

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the article is no longer returned in the list
        var getResponse = await httpClient.GetAsync("/articles");
        Assert.True(getResponse.IsSuccessStatusCode);

        var articles = await getResponse.Content.ReadFromJsonAsync<ArticlesPageDto>();
        Assert.NotNull(articles);
        Assert.Empty(articles.Items);
    }

    [Fact]
    public async Task DeleteArticle_ReturnsBadRequest_WhenArticleDoesNotExist()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.DeleteAsync("/articles/nonexistent-id");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterArticle_ReturnsCreated_WhenArticleIsRegistered()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles",
            new RegisterArticleRequestDto("Some test article", 1000));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();

        Assert.NotNull(entityPointer);

        Assert.NotEmpty(entityPointer.Id);
        Assert.NotEmpty(entityPointer.Location);

        Assert.Equal($"/articles/{entityPointer.Id}", entityPointer.Location);
    }
}