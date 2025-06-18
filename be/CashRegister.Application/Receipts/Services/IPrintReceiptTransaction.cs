using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Services;

public interface IPrintReceiptTransaction
{
    Task PrintReceiptAsync(Identifier orderId);
}