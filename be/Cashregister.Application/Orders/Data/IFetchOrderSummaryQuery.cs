using Cashregister.Application.Orders.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Application.Orders.Data;

public interface IFetchOrderSummaryQuery
{
    Task<OrderSummary?> FetchAsync(Identifier orderId);
}