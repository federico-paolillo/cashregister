using Cashregister.Domain;

namespace Cashregister.Application.Articles.Data;

public interface ISaveArticleCommand
{
    Task SaveAsync(Article article);

    Task SaveAsync(RetiredArticle retiredArticle);
}