using Cashregister.Domain;

namespace Cashregister.Api.Articles.Models;

public sealed record ArticleDto(
    string Id,
    string Description,
    decimal Price
)
{
    public static ArticleDto From(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new ArticleDto(
            article.Id.Value,
            article.Description,
            article.Price.AsPayableMoney()
        );
    }
}