using Cashregister.Application.Orders.Commands;
using Cashregister.Database.Entities;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

namespace Cashregister.Database.Commands;

public sealed class SaveOrderCommand(
    IApplicationDbContext applicationDbContext,
    OrderEntityMapper orderEntityMapper
) : ISaveOrderCommand
{
    public async Task SaveAsync(PendingOrder newPendingOrder)
    {
        var orderEntity = orderEntityMapper.ToEntity(newPendingOrder);

        await applicationDbContext.Orders.AddAsync(orderEntity);
    }
}