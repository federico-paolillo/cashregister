using Cashregister.Domain;

namespace Cashregister.Application.Orders.Data;

public interface IFetchArticlesQuery
{
    Task<Article[]> FetchAsync(params Identifier[] identifiers);
}