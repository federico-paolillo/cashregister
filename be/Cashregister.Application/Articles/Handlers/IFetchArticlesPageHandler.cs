using Cashregister.Application.Articles.Models.Output;
using Cashregister.Application.Pagination;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Handlers;

public interface IFetchArticlesPageHandler
{
    Task<Result<Page<ArticleListItem>>> ExecuteAsync(PageRequest pageRequest);
}