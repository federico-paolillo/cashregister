using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Models.Output;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions;

public interface IFetchArticlesPageTransaction
{
  Task<Result<ArticlesPage>> ExecuteAsync(ArticlesPageRequest pageRequest);
}