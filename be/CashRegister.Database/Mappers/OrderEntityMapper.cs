using CashRegister.Database.Entities;
using CashRegister.Domain;

namespace CashRegister.Database.Mappers;

public sealed class OrderEntityMapper
{
    public OrderEntity ToEntity(PendingOrder pendingOrder)
    {
        var itemEntities = pendingOrder.Items
            .Select(i => new OrderItemEntity
            {
                Id = i.Id.Value,
                ArticleId = i.Article.Value,
                OrderId = pendingOrder.Id.Value,
                Description = i.Description,
                Price = i.Price.Value
            })
            .ToList();

        return new OrderEntity
        {
            Id = pendingOrder.Id.Value,
            Date = pendingOrder.Date.Value,
            Items = itemEntities,
        };
    }
}