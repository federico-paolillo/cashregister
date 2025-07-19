using Cashregister.Domain;

namespace Cashregister.Application.Orders.Commands;

public interface ISaveOrderCommand
{
    Task SaveAsync(PendingOrder newPendingOrder);
}