using Cashregister.Domain;

namespace Cashregister.Application.Orders.Data;

public interface IFetchOrderQuery
{
    Task<Order?> FetchAsync(Identifier orderId);
}
