using System.Collections.Immutable;

using Cashregister.Application.Articles.Models.Output;
using Cashregister.Domain;

namespace Cashregister.Application.Articles.Data;

public interface IFetchArticlesListQuery
{
    Task<ImmutableArray<ArticleListItem>> FetchAsync(uint count, Identifier? after = null);
}