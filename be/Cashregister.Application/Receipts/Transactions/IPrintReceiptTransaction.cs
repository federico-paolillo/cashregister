using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Receipts.Transactions;

public interface IPrintReceiptTransaction
{
    Task<Result<Unit>> ExecuteAsync(Identifier orderId, CancellationToken cancellationToken = default);
}