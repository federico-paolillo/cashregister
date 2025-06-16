using CashRegister.Application.Receipts.Models.Output;
using CashRegister.Domain;

namespace CashRegister.Application.Receipts.Queries;

public interface IFetchOrderPrintDataQuery
{
    public Task<OrderPrintData> Fetch(Identifier orderId);
}