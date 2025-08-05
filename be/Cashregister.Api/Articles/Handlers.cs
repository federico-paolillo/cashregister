using System.Collections.Immutable;

using Cashregister.Api.Articles.Models;
using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Domain;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.Articles;

internal static class Handlers
{
  public static async Task<Results<BadRequest, Ok<ArticlePageDto>>> GetArticlesPage(
    IFetchArticlesPageTransaction fetchArticlesPageTransaction,
    [FromQuery(Name = "pageSize")] uint pageSize = 50,
    [FromQuery(Name = "after")] string? after = null
  )
  {
    var afterIdentifier = after is not null ? Identifier.From(after) : null;

    var pageRequest = new ArticlesPageRequest
    {
      After = afterIdentifier,
      Size = pageSize,
    };

    var articlesPageResult = await fetchArticlesPageTransaction.ExecuteAsync(pageRequest);

    if (articlesPageResult.NotOk)
    {
      return TypedResults.BadRequest();
    }

    var articlesPage = articlesPageResult.Value;

    var articlesListItemDto = articlesPage.Articles
      .Select(ArticleListItemDto.From)
      .ToImmutableArray();

    var articlesPageDto = new ArticlePageDto(
      articlesPage.Next?.ToString(),
      articlesPage.HasNext,
      articlesListItemDto
    );

    return TypedResults.Ok(articlesPageDto);
  }

  public static async Task<Results<BadRequest, Created<RegisterArticleRequestDto>>> RegisterArticle(
    IRegisterArticleTransaction registerArticleTransaction,
    [FromBody] RegisterArticleRequestDto request
  )
  {
    var articleDefinition = new ArticleDefinition
    {
      Description = request.Description,
      Price = Cents.From(request.PriceInCents)
    };

    var result = await registerArticleTransaction.ExecuteAsync(articleDefinition);

    if (result.NotOk)
    {
      return TypedResults.BadRequest();
    }

    return TypedResults.Created($"/articles/{result.Value}", request);
  }

  public static async Task<Results<BadRequest, NotFound, Ok<ArticleDto>>> GetArticle(
    IFetchArticleQuery fetchArticleQuery,
    [FromRoute] string id
  )
  {
    var identifier = Identifier.From(id);

    var article = await fetchArticleQuery.FetchAsync(identifier);

    if (article is null)
    {
      return TypedResults.NotFound();
    }

    var articleDto = ArticleDto.From(article);

    return TypedResults.Ok(articleDto);
  }
}
