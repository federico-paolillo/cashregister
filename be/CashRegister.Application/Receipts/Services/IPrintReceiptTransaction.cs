using CashRegister.Domain;

namespace CashRegister.Application.Receipts.Services;

public interface IPrintReceiptTransaction
{
    Task PrintReceiptAsync(Order order); 
}