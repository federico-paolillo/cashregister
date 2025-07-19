using Cashregister.Application.Articles.Models.Input;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions;

public interface IRegisterArticleTransaction
{
    Task<Result<Identifier>> ExecuteAsync(
        ArticleDefinition articleDefinition,
        CancellationToken cancellationToken = default
    );
}