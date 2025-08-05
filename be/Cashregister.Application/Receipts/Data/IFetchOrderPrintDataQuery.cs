using Cashregister.Application.Receipts.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Data;

public interface IFetchOrderPrintDataQuery
{
    public Task<OrderPrintData> Fetch(Identifier orderId);
}