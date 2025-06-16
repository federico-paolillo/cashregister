using CashRegister.Application.Orders.Commands;
using CashRegister.Database.Mappers;
using CashRegister.Domain;

namespace CashRegister.Database.Commands;

public sealed class SaveOrderCommand(
    IApplicationDbContext applicationDbContext,
    OrderEntityMapper orderEntityMapper
) : ISaveOrderCommand
{
    public async Task SaveAsync(Order newOrder)
    {
        var orderEntity = orderEntityMapper.ToEntity(newOrder);
        
        await applicationDbContext.Orders.AddAsync(orderEntity);
    }
}