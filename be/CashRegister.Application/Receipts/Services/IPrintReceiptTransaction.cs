using CashRegister.Application.Receipts.Queries;
using CashRegister.Domain;

namespace CashRegister.Application.Receipts.Services;

public interface IPrintReceiptTransaction
{
    Task PrintReceiptAsync(Identifier orderId);
}