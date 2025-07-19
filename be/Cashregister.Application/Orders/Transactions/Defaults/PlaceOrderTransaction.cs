using System.Collections.Immutable;

using Cashregister.Application.Orders.Commands;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Queries;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Orders.Transactions.Defaults;

public sealed class PlaceOrderTransaction(
    IFetchArticlesQuery fetchArticlesQuery,
    ISaveOrderCommand saveOrderCommand,
    IUnitOfWork unitOfWork
) :
    Transaction<OrderRequest, Identifier>(unitOfWork),
    IPlaceOrderTransaction
{
    protected override async Task<Result<Identifier>> InternalExecuteAsync(OrderRequest orderRequest)
    {
        ArgumentNullException.ThrowIfNull(orderRequest);

        Identifier[] articlesRequested = orderRequest.Items
            .Select(item => item.Article)
            .ToArray();

        Article[] articles = await fetchArticlesQuery.FetchAsync(articlesRequested);

        ImmutableArray<Item> orderItems =
        [
            ..articles
                .Select(a => new Item
                {
                    Id = Identifier.New(),
                    Article = a.Id,
                    Description = a.Description,
                    Price = a.Price,
                    Quantity = 0 // TODO
                })
        ];

        PendingOrder pendingOrder = new()
        {
            Id = Identifier.New(),
            Date = TimeStamp.Now(),
            Items = orderItems
        };

        await saveOrderCommand.SaveAsync(pendingOrder);

        return Result.Ok(pendingOrder.Id);
    }
}