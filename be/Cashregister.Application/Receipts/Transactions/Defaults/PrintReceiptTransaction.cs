using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Receipts.Transactions.Defaults;

public sealed class PrintReceiptTransaction(
    IUnitOfWork unitOfWork
) :
    Transaction<Identifier, Unit>(unitOfWork),
    IPrintReceiptTransaction
{
    protected override Task<Result<Unit>> InternalExecuteAsync(Identifier orderId)
    {
        // TODO: Implement actual receipt printing

        var result = Result.Void();

        return Task.FromResult(result);
    }
}