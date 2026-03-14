using Cashregister.Application.Orders.Data;
using Cashregister.Database.Mappers;
using Cashregister.Domain;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database.Queries;

public sealed class FetchOrderQuery(
    IApplicationDbContext dbContext,
    OrderEntityMapper orderEntityMapper
) : IFetchOrderQuery
{
    public async Task<Order?> FetchAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        var maybeOrderEntity = await dbContext.Orders
            .Include(x => x.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == orderId.Value);

        if (maybeOrderEntity is null)
        {
            return null;
        }

        return orderEntityMapper.FromEntity(maybeOrderEntity);
    }
}
