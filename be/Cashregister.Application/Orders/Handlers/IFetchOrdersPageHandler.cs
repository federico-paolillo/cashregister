using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Factories;

namespace Cashregister.Application.Orders.Handlers;

public interface IFetchOrdersPageHandler
{
    Task<Result<Page<OrderListItem>>> ExecuteAsync(PageRequest pageRequest);
}
