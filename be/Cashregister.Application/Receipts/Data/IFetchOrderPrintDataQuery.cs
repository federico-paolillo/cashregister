using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Data;

/// <summary>
///     Fetches the receipt-specific order projection used by receipt print program builders.
/// </summary>
public interface IFetchOrderPrintDataQuery
{
    Task<OrderPrintData?> FetchAsync(Identifier orderId);
}