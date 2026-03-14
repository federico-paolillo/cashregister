using Cashregister.Application.Orders.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Factories;

namespace Cashregister.Application.Orders.Handlers.Defaults;

public sealed class FetchOrdersPageHandler(
  IPaginationQuery<OrderListItem> ordersListFetcher
) : IFetchOrdersPageHandler
{
    public async Task<Result<Page<OrderListItem>>> ExecuteAsync(PageRequest pageRequest)
    {
        return await Paginator.FetchPageAsync(ordersListFetcher, pageRequest);
    }
}
