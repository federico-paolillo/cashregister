using System.Collections.Immutable;

using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Pagination;

public static class Paginator
{
    public static async Task<Result<Page<T>>> FetchPageAsync<T>(
        IPaginationQuery<T> query,
        PageRequest pageRequest) where T : class, IPageItem
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(pageRequest);

        if (pageRequest.Until is not null)
        {
            return await FetchUntilPageAsync(query, pageRequest.Until, pageRequest.Size);
        }

        var pageSizePlusOne = pageRequest.Size + 1;

        var itemsPlusOne = await query.FetchAsync(pageSizePlusOne, pageRequest.After);

        var hasMore = itemsPlusOne.Length > pageRequest.Size;

        var integerPageSize = (int)pageRequest.Size;

        var actualItems = itemsPlusOne
          .Take(integerPageSize)
          .ToImmutableArray();

        var maybeNext = hasMore && actualItems.Length > 0
            ? actualItems[^1]
            : null;

        var page = new Page<T>
        {
            Items = actualItems,
            HasNext = hasMore,
            Next = maybeNext?.Id
        };

        return Result.Ok(page);
    }

    private static async Task<Result<Page<T>>> FetchUntilPageAsync<T>(
        IPaginationQuery<T> query,
        Identifier until,
        uint pageSize) where T : class, IPageItem
    {
        var existingItems = await query.FetchUntilAsync(until);

        var nextPagePlusOne = await query.FetchAsync(pageSize + 1, until);
        var hasMore = nextPagePlusOne.Length > pageSize;
        var nextPageItems = nextPagePlusOne.Take((int)pageSize).ToImmutableArray();
        var maybeNext = hasMore && nextPageItems.Length > 0
            ? nextPageItems[^1]
            : null;

        var page = new Page<T>
        {
            Items = existingItems.AddRange(nextPageItems),
            HasNext = hasMore,
            Next = maybeNext?.Id
        };

        return Result.Ok(page);
    }
}
