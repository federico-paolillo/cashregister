using CashRegister.Application.Orders.Models.Input;
using CashRegister.Domain;

namespace CashRegister.Application.Orders.Transactions;

public interface IPlaceOrderTransaction
{
    Task<Identifier> PlaceOrderAsync(OrderRequest orderRequest); 
}