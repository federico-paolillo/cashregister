using Cashregister.Domain;

namespace Cashregister.Application.Orders.Queries;

public interface IFetchArticlesQuery
{
    Task<Article[]> FetchAsync(params Identifier[] identifiers);
}