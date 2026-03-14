using Cashregister.Application.Orders.Models.Output;
using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Mappers;

public sealed class OrderListItemMapper
{
    public OrderListItem FromEntity(OrderEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new OrderListItem
        {
            Id = Identifier.From(entity.Id),
            Number = OrderNumber.From(entity.RowId),
            Date = TimeStamp.From(entity.Date),
            Total = Cents.From(entity.Items.Sum(i => (long)(i.Quantity * i.Price)))
        };
    }
}
