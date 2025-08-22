using System.Collections.Immutable;

using Cashregister.Api.Articles.Models;
using Cashregister.Api.Commons.Models;
using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Problems;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Domain;
using Cashregister.Factories.Problems;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.Articles;

internal static class Handlers
{
  public static async Task<Results<BadRequest, Ok<ArticlesPageDto>>> GetArticlesPage(
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

    var articlesPageDto = new ArticlesPageDto(
      articlesPage.Next?.Value,
      articlesPage.HasNext,
      articlesListItemDto
    );

    return TypedResults.Ok(articlesPageDto);
  }

  public static async Task<Results<BadRequest, Created<EntityPointerDto>>> RegisterArticle(
    IRegisterArticleTransaction registerArticleTransaction,
    LinkGenerator linkGenerator,
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

    var location = linkGenerator.GetPathByName("GetArticle", new { id = result.Value.Value })
        ?? throw new InvalidOperationException("Failed to generate location for article");

    var orderPointerDto = new EntityPointerDto
    {
      Id = result.Value.Value,
      Location = location
    };

    return TypedResults.Created(location, orderPointerDto);
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

  public static async Task<Results<NotFound, NoContent>> DeleteArticle(
    IRetireArticleTransaction retireArticleTransaction,
    [FromRoute] string id
  )
  {
    var identifier = Identifier.From(id);

    var result = await retireArticleTransaction.ExecuteAsync(identifier);

    if (result.NotOk)
    {
      if (result.Error is NoSuchArticleProblem)
      {
        return TypedResults.NotFound();
      }
    }

    return TypedResults.NoContent();
  }
}
