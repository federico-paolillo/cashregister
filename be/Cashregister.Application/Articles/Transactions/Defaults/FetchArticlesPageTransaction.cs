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
            return await FetchUntilPageAsync(pageRequest.Until);
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

    private async Task<Result<ArticlesPage>> FetchUntilPageAsync(Identifier until)
    {
        var articlesBeforeUntil = await articlesListFetcher.FetchUntilAsync(until);

        // Find what cursor the client should use to continue scrolling forward.
        // FetchAsync with count=1 and after=until returns the first article at or after the cursor.
        var articlesAtOrAfterUntil = await articlesListFetcher.FetchAsync(1, until);
        var nextArticle = articlesAtOrAfterUntil.Length > 0 ? articlesAtOrAfterUntil[0] : null;

        var articlesPage = new ArticlesPage
        {
            Articles = articlesBeforeUntil,
            HasNext = nextArticle is not null,
            Next = nextArticle?.Id
        };

        return Result.Ok(articlesPage);
    }
}