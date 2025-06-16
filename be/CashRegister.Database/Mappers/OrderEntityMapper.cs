using CashRegister.Database.Entities;
using CashRegister.Domain;

namespace CashRegister.Database.Mappers;

public sealed class OrderEntityMapper
{
    public OrderEntity ToEntity(Order order)
    {
        var itemEntities = order.Items
            .Select(i => new OrderItemEntity
            {
                Id = i.Id.Value,
                ArticleId = i.Article.Value,
                OrderId = order.Id.Value,
                Description = i.Description,
                Price = i.Price.Value
            })
            .ToList();

        return new OrderEntity
        {
            Id = order.Id.Value,
            Date = order.Date.Value,
            Items = itemEntities
        };
    }
}