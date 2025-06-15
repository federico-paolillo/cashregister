using CashRegister.Application.Orders.Commands;
using CashRegister.Application.Orders.Queries;
using CashRegister.Application.Orders.Transactions.Models.Input;
using CashRegister.Domain;

namespace CashRegister.Application.Orders.Transactions.Defaults;

public sealed class PlacePlaceOrderTransaction(
    IFetchArticlesQuery fetchArticlesQuery,
    ISaveOrderCommand saveOrderCommand,
    IUnitOfWork unitOfWork
) : IPlaceOrderTransaction
{
    public async Task<Order> PlaceOrderAsync(OrderRequest orderRequest)
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

        var orderToPersist = new Order
        {
            Id = Identifier.New(),
            Date = TimeStamp.Now(),
            Items = orderItems
        };

        await saveOrderCommand.SaveAsync(orderToPersist);

        await unitOfWork.SaveChangesAsync();

        return orderToPersist;
    }
}