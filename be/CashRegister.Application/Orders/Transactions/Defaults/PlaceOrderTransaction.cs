using CashRegister.Application.Orders.Commands;
using CashRegister.Application.Orders.Models.Input;
using CashRegister.Application.Orders.Queries;
using CashRegister.Domain;

namespace CashRegister.Application.Orders.Transactions.Defaults;

public sealed class PlaceOrderTransaction(
    IFetchArticlesQuery fetchArticlesQuery,
    ISaveOrderCommand saveOrderCommand,
    IUnitOfWork unitOfWork
) : IPlaceOrderTransaction
{
    public async Task<Identifier> PlaceOrderAsync(OrderRequest orderRequest)
    {
        var articlesRequested = orderRequest.Items
            .Select(item => item.Article)
            .ToArray();

        var articles = await fetchArticlesQuery.FetchAsync(articlesRequested);

        var orderItems = articles
            .Select(a => new Item
            {
                Id = Identifier.New(),
                Article = a.Id,
                Description = a.Description,
                Price = a.Price,
                Quantity = 0 // TODO
            })
            .ToArray();

        var pendingOrder = new PendingOrder
        {
            Id = Identifier.New(),
            Date = TimeStamp.Now(),
            Items = orderItems
        };

        await saveOrderCommand.SaveAsync(pendingOrder);

        await unitOfWork.SaveChangesAsync();

        return pendingOrder.Id;
    }
}