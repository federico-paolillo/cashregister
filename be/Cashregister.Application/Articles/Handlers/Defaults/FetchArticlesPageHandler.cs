using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Handlers.Defaults;

public sealed class FetchArticlesPageHandler(
    IPaginationQuery<ArticleListItem> articlesListFetcher
) : IFetchArticlesPageHandler
{
    public async Task<Result<Page<ArticleListItem>>> ExecuteAsync(PageRequest pageRequest)
    {
        return await Paginator.FetchPageAsync(articlesListFetcher, pageRequest);
    }
}