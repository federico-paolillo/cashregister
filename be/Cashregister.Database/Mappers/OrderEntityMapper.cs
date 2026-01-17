using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Mappers;

public sealed class OrderEntityMapper
{
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