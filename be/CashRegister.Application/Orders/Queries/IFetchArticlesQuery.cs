using CashRegister.Domain;

namespace CashRegister.Application.Orders.Queries;

public interface IFetchArticlesQuery
{
    Task<Article[]> FetchAsync(params Identifier[] identifiers);
}