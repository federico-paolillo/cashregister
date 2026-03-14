using System.Collections.Immutable;

using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Mappers;

public sealed class OrderEntityMapper
{
    public Order FromEntity(OrderEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        ImmutableArray<Item> items = [
            .. entity.Items.Select(i => new Item
            {
                Id = Identifier.From(i.Id),
                Article = Identifier.From(i.ArticleId),
                Description = i.Description,
                Price = Cents.From(i.Price),
                Quantity = i.Quantity
            })
        ];

        return new Order
        {
            Id = Identifier.From(entity.Id),
            Number = OrderNumber.From(entity.RowId),
            Date = TimeStamp.From(entity.Date),
            Items = items
        };
    }

    public OrderEntity ToEntity(PendingOrder pendingOrder)
    {
        ArgumentNullException.ThrowIfNull(pendingOrder);

        List<OrderItemEntity> itemEntities = [
            .. pendingOrder.Items.Select(i => new OrderItemEntity
            {
                Id = i.Id.Value,
                ArticleId = i.Article.Value,
                OrderId = pendingOrder.Id.Value,
                Description = i.Description,
                Price = i.Price.Value,
                Quantity = i.Quantity
            })
        ];

        return new OrderEntity
        {
            Id = pendingOrder.Id.Value,
            Date = pendingOrder.Date.Value,
            Items = itemEntities
        };
    }
}