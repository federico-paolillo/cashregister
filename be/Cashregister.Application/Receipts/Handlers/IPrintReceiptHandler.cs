using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Receipts.Handlers;

/// <summary>
///     Prints the receipt for an existing order.
/// </summary>
public interface IPrintReceiptHandler
{
    Task<Result<Unit>> ExecuteAsync(Identifier orderId);
}