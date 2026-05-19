using System.Collections.Immutable;

using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon;

namespace Cashregister.Application.Receipts.Services;

/// <summary>
///     Builds receipt print programs from receipt-specific order projections.
/// </summary>
public interface IReceiptPrintProgramService
{
    Task<Result<ImmutableArray<PrintProgram>>> BuildAsync(Identifier orderId);
}