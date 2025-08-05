using Cashregister.Domain;

namespace Cashregister.Application.Orders.Data;

public interface ISaveOrderCommand
{
    Task SaveAsync(PendingOrder newPendingOrder);
}