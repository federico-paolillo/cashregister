using Cashregister.Domain;

namespace Cashregister.Api.Articles.Models;

internal sealed record ArticleDto(
    string Id,
    string Description,
    decimal Price
)
{
  public static ArticleDto From(Article article)
  {
    return new ArticleDto(
        article.Id.ToString(),
        article.Description,
        article.Price.AsPayableMoney()
    );
  }
}