using Cashregister.Application.Orders.Models.Input;
using Cashregister.Domain;

namespace Cashregister.Application.Orders.Transactions;

public interface IPlaceOrderTransaction
{
    Task<Identifier> PlaceOrderAsync(OrderRequest orderRequest);
}