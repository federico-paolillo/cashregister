using Cashregister.Application.Articles.Models.Input;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions;

public interface IChangeArticleTransaction
{
    Task<Result<Unit>> ExecuteAsync(ArticleChange change, CancellationToken cancellationToken = default);
}