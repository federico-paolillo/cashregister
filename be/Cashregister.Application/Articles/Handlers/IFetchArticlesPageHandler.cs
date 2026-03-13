using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Handlers;

public interface IFetchArticlesPageHandler
{
    Task<Result<ArticlesPage>> ExecuteAsync(ArticlesPageRequest pageRequest);
}
