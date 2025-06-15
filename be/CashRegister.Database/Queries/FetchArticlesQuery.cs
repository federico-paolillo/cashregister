using CashRegister.Application.Orders.Queries;
using CashRegister.Database.Entities;
using CashRegister.Domain;

namespace CashRegister.Database.Queries;

public sealed class FetchArticlesQuery(ApplicationDbContext dbContext) : IFetchArticlesQuery
{
    public Task<Article[]> FetchAsync(params Identifier[] identifiers)
    {
        return Task.FromResult<Article[]>([]);
    }
}