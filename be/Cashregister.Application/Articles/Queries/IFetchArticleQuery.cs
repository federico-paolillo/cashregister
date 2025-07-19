using Cashregister.Domain;

namespace Cashregister.Application.Articles.Queries;

public interface IFetchArticleQuery
{
    Task<Article?> FetchAsync(Identifier identifier);
}