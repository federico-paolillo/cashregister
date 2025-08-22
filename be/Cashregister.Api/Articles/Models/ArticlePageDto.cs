using System.Collections.Immutable;

using Cashregister.Application.Articles.Models.Output;

namespace Cashregister.Api.Articles.Models;

public sealed record ArticlesPageDto(
  string? Next,
  bool HasNext,
  ImmutableArray<ArticleListItemDto> Items
);

public sealed record ArticleListItemDto(
  string Id,
  string Description,
  decimal Price
)
{
  public static ArticleListItemDto From(ArticleListItem article)
  {
    ArgumentNullException.ThrowIfNull(article);

    return new ArticleListItemDto(
      article.Id.Value,
      article.Description,
      article.Price.AsPayableMoney()
    );
  }
}
