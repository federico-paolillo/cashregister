using CashRegister.Application.Orders.Models.Output;
using CashRegister.Domain;

namespace CashRegister.Application.Orders.Queries;

public interface IFetchOrderSummaryQuery
{
    Task<OrderSummary?> FetchAsync(Identifier orderId);
}