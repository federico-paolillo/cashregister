using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;

namespace Cashregister.Application.Receipts.Services;

/// <summary>
///     Builds receipt print programs from receipt-specific order projections.
/// </summary>
public interface IReceiptPrintProgramService
{
    Task<Result<PrintProgram>> BuildAsync(Identifier orderId);
}