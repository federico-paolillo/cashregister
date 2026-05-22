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
    public async Task GetArticles_WithUntil_ReturnsAccumulatedViewPlusOneMorePage()
    {
        await PrepareEnvironmentAsync();

        var article1Result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article 1",
                Price = Cents.From(100L)
            })
        );

        var article2Result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article 2",
                Price = Cents.From(200L)
            })
        );

        var article3Result = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article 3",
                Price = Cents.From(300L)
            })
        );

        Assert.True(article1Result.Ok);
        Assert.True(article2Result.Ok);
        Assert.True(article3Result.Ok);

        using var httpClient = CreateHttpClient();

        // First get page 1 to obtain the Next cursor
        var page1Response = await httpClient.GetAsync("/articles?pageSize=1");
        Assert.True(page1Response.IsSuccessStatusCode);

        var page1 = await page1Response.Content.ReadFromJsonAsync<ArticlesPageDto>();
        Assert.NotNull(page1);
        Assert.NotNull(page1.Next);

        // Use the Next cursor from page 1 as the until parameter
        // pageSize=1: page 1 returns [Article 1], next=article1_id (last item of page).
        // until=article1_id should return [Article 1, Article 2] with next=article2_id.
        var response = await httpClient.GetAsync($"/articles?pageSize=1&until={page1.Next}");

        Assert.True(response.IsSuccessStatusCode);

        var page = await response.Content.ReadFromJsonAsync<ArticlesPageDto>();

        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Length);
        Assert.Equal(article1Result.Value.Value, page.Items[0].Id);
        Assert.Equal(article2Result.Value.Value, page.Items[1].Id);
        Assert.True(page.HasNext);
        Assert.Equal(article2Result.Value.Value, page.Next);
    }

    [Fact]
    public async Task GetArticles_WithBothAfterAndUntil_ReturnsBadRequest()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/articles?after=somecursor&until=anothercursor");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterArticle_ReturnsCreated_WhenArticleIsRegistered()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles",
            new RegisterArticleRequestDto("Some test article", 1000, true, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();

        Assert.NotNull(entityPointer);

        Assert.NotEmpty(entityPointer.Id);
        Assert.NotEmpty(entityPointer.Location);

        Assert.Equal($"/articles/{entityPointer.Id}", entityPointer.Location);
    }

    [Fact]
    public async Task RegisterAndGetArticle_ShouldPreserveOddCentPrice()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles",
            new RegisterArticleRequestDto("Odd cents article", 101, true, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();
        Assert.NotNull(entityPointer);

        var getResponse = await httpClient.GetAsync($"/articles/{entityPointer.Id}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.Equal(101L, article.PriceInCents);
        Assert.True(article.PrintDetailReceipt);
        Assert.Null(article.QuantityAvailable);
    }

    [Fact]
    public async Task RegisterAndGetArticle_PreservesDetailReceiptSelection()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles",
            new RegisterArticleRequestDto("Overview only article", 101, false, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();
        Assert.NotNull(entityPointer);

        var getResponse = await httpClient.GetAsync($"/articles/{entityPointer.Id}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.False(article.PrintDetailReceipt);
    }

    [Fact]
    public async Task RegisterAndGetArticle_PreservesQuantityAvailable()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles",
            new RegisterArticleRequestDto("Tracked article", 101, true, 15));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var entityPointer = await response.Content.ReadFromJsonAsync<EntityPointerDto>();
        Assert.NotNull(entityPointer);

        var listResponse = await httpClient.GetAsync("/articles");
        Assert.True(listResponse.IsSuccessStatusCode);

        var page = await listResponse.Content.ReadFromJsonAsync<ArticlesPageDto>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal(15, page.Items[0].QuantityAvailable);

        var getResponse = await httpClient.GetAsync($"/articles/{entityPointer.Id}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.Equal(15, article.QuantityAvailable);
    }

    [Fact]
    public async Task RegisterArticle_ReturnsBadRequest_WhenDetailReceiptSelectionIsMissing()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync("/articles", new
        {
            description = "Missing detail receipt flag",
            priceInCents = 101
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeArticle_UpdatesDetailReceiptSelection()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Configurable article", 500);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync($"/articles/{articleId.Value}",
            new ChangeArticleRequestDto("Configurable article", 500, false, null));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await httpClient.GetAsync($"/articles/{articleId.Value}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.False(article.PrintDetailReceipt);
    }

    [Fact]
    public async Task ChangeArticle_UpdatesAndDisablesQuantityAvailable()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Quantity article", 500);

        using var httpClient = CreateHttpClient();

        var updateResponse = await httpClient.PostAsJsonAsync($"/articles/{articleId.Value}",
            new ChangeArticleRequestDto("Quantity article", 500, true, -2));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await httpClient.GetAsync($"/articles/{articleId.Value}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.Equal(-2, article.QuantityAvailable);

        var disableResponse = await httpClient.PostAsJsonAsync($"/articles/{articleId.Value}",
            new ChangeArticleRequestDto("Quantity article", 500, true, null));

        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        getResponse = await httpClient.GetAsync($"/articles/{articleId.Value}");
        Assert.True(getResponse.IsSuccessStatusCode);

        article = await getResponse.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.Null(article.QuantityAvailable);
    }

    [Fact]
    public async Task ChangeArticle_ReturnsBadRequest_WhenDetailReceiptSelectionIsMissing()
    {
        await PrepareEnvironmentAsync();

        var articleId = await CreateArticleAsync("Configurable article", 500);

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsJsonAsync($"/articles/{articleId.Value}", new
        {
            description = "Configurable article",
            priceInCents = 500
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetArticle_ReturnsRawIdString_NotRecordToString()
    {
        await PrepareEnvironmentAsync();

        var registerResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = "Article for Id test",
                Price = Cents.From(500L)
            })
        );

        Assert.True(registerResult.Ok);

        var expectedId = registerResult.Value.Value;

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync($"/articles/{expectedId}");

        Assert.True(response.IsSuccessStatusCode);

        var article = await response.Content.ReadFromJsonAsync<ArticleDto>();

        Assert.NotNull(article);
        Assert.Equal(expectedId, article.Id);
        // Ensure it's the raw ULID string, not the record ToString() format like "Identifier { Value = ... }"
        Assert.DoesNotContain("Identifier", article.Id, StringComparison.Ordinal);
    }

    private async Task<Identifier> CreateArticleAsync(string description, long priceInCents)
    {
        var registerResult = await RunScoped<IRegisterArticleTransaction, Result<Identifier>>(tx =>
            tx.ExecuteAsync(new ArticleDefinition
            {
                Description = description,
                Price = Cents.From(priceInCents)
            }));

        Assert.True(registerResult.Ok);
        return registerResult.Value;
    }
}