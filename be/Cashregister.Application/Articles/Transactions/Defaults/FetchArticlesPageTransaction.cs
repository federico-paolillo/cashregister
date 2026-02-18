using System.Collections.Immutable;

using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions.Defaults;

public sealed class FetchArticlesPageTransaction(
  IFetchArticlesListQuery articlesListFetcher
) : IFetchArticlesPageTransaction
{
    public async Task<Result<ArticlesPage>> ExecuteAsync(ArticlesPageRequest pageRequest)
    {
        ArgumentNullException.ThrowIfNull(pageRequest);

        if (pageRequest.Until is not null)
        {
            return await FetchUntilPageAsync(pageRequest.Until, pageRequest.Size);
        }

        var pageSizePlusOne = pageRequest.Size + 1;

        var articleListItemPlusOne = await articlesListFetcher.FetchAsync(pageSizePlusOne, pageRequest.After);

        var hasMore = articleListItemPlusOne.Length > pageRequest.Size;

        var integerPageSize = (int)pageRequest.Size;

        var maybeNext = hasMore ? articleListItemPlusOne[^1] : null;

        var actualArticleListItems = articleListItemPlusOne
          .Take(integerPageSize)
          .ToImmutableArray();

        var articlesPage = new ArticlesPage
        {
            Articles = actualArticleListItems,
            HasNext = hasMore,
            Next = maybeNext?.Id
        };

        return Result.Ok(articlesPage);
    }

    private async Task<Result<ArticlesPage>> FetchUntilPageAsync(Identifier until, uint pageSize)
    {
        var existingItems = await articlesListFetcher.FetchUntilAsync(until);

        var nextPagePlusOne = await articlesListFetcher.FetchAsync(pageSize + 1, until);
        var hasMore = nextPagePlusOne.Length > pageSize;
        var nextPageItems = nextPagePlusOne.Take((int)pageSize).ToImmutableArray();
        var maybeNext = hasMore ? nextPagePlusOne[^1] : null;

        var articlesPage = new ArticlesPage
        {
            Articles = existingItems.AddRange(nextPageItems),
            HasNext = hasMore,
            Next = maybeNext?.Id
        };

        return Result.Ok(articlesPage);
    }
}