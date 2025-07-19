using Cashregister.Domain;

namespace Cashregister.Application.Articles.Commands;

public interface ISaveArticleCommand
{
    Task SaveAsync(Article newArticle);

    Task SaveAsync(RetiredArticle retiredArticle);
}