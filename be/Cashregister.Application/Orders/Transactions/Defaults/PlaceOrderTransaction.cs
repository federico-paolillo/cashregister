using System.Collections.Immutable;

using Cashregister.Application.Orders.Data;
using Cashregister.Application.Orders.Models.Input;
using Cashregister.Application.Orders.Problems;
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

        Identifier[] articlesRequested = [.. orderRequest.Items.Select(item => item.Article)];

        Article[] articles = await fetchArticlesQuery.FetchAsync(articlesRequested);

        if (articles.Length != orderRequest.Items.Length)
        {
            return Result.Error<Identifier>(new OrderRequestIsMissingSomeArticles([.. articlesRequested]));
        }

        var articlesWithQuantity =
            from item in orderRequest.Items
            join article in articles on item.Article.Value equals article.Id.Value
            select new
            {
                Article = article,
                item.Quantity
            };


        ImmutableArray<Item> orderItems =
        [
            ..articlesWithQuantity
                .Select(a => new Item
                {
                    Id = Identifier.New(),
                    Article = a.Article.Id,
                    Description = a.Article.Description,
                    Price = a.Article.Price,
                    Quantity = a.Quantity
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