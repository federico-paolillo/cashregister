using System.Collections.Immutable;

using Cashregister.Application.Articles.Models.Output;

namespace Cashregister.Api.Articles.Models;

internal sealed record ArticlePageDto(
  string? Next,
  bool HasNext,
  ImmutableArray<ArticleListItemDto> Items
);

internal sealed record ArticleListItemDto(
  string Id,
  string Description,
  decimal Price
)
{
  public static ArticleListItemDto From(ArticleListItem article)
  {
    return new ArticleListItemDto(
      article.Id.ToString(),
      article.Description,
      article.Price.AsPayableMoney()
    );
  }
}
