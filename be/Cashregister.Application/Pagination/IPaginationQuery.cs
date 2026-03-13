using System.Collections.Immutable;

using Cashregister.Domain;

namespace Cashregister.Application.Pagination;

public interface IPaginationQuery<T> where T : class, IPageItem
{
    Task<ImmutableArray<T>> FetchAsync(uint count, Identifier? after = null);

    Task<ImmutableArray<T>> FetchUntilAsync(Identifier until);
}
