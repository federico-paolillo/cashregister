using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Data;

public interface IFetchOrderPrintDataQuery
{
    Task<OrderPrintData> Fetch(Identifier orderId);
}