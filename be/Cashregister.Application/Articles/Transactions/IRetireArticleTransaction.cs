using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions;

public interface IRetireArticleTransaction
{
    Task<Result<Unit>> ExecuteAsync(
        Identifier articleId,
        CancellationToken cancellationToken = default
    );
}