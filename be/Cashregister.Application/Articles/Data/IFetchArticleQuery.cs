using Cashregister.Domain;

namespace Cashregister.Application.Articles.Data;

public interface IFetchArticleQuery
{
    Task<Article?> FetchAsync(Identifier identifier);
}