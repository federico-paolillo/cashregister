using System.Collections.Immutable;

using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
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
            return await FetchUntilAsync(pageRequest.Until);
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

    private async Task<Result<ArticlesPage>> FetchUntilAsync(Identifier until)
    {
        var itemsBeforeUntil = await articlesListFetcher.FetchAllBeforeAsync(until);

        var peek = await articlesListFetcher.FetchAsync(1, until);
        var hasMore = peek.Length > 0;

        var articlesPage = new ArticlesPage
        {
            Articles = itemsBeforeUntil,
            HasNext = hasMore,
            Next = hasMore ? until : null
        };

        return Result.Ok(articlesPage);
    }
}