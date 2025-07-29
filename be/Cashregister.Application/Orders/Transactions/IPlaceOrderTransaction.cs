using Cashregister.Application.Orders.Models.Input;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Orders.Transactions;

public interface IPlaceOrderTransaction
{
    Task<Result<Identifier>> ExecuteAsync(OrderRequest orderRequest, CancellationToken cancellationToken = default);
}