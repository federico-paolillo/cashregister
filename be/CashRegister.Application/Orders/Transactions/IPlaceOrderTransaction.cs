using CashRegister.Application.Orders.Transactions.Models.Input;
using CashRegister.Domain;

namespace CashRegister.Application.Orders.Transactions;

public interface IPlaceOrderTransaction
{
    Task<Order> PlaceOrderAsync(OrderRequest orderRequest); 
}