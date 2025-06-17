using CashRegister.Domain;

namespace CashRegister.Application.Orders.Commands;

public interface ISaveOrderCommand
{
    Task SaveAsync(PendingOrder newPendingOrder);
}