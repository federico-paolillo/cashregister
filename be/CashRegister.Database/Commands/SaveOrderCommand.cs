using CashRegister.Application.Orders.Commands;
using CashRegister.Database.Mappers;
using CashRegister.Domain;

namespace CashRegister.Database.Commands;

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