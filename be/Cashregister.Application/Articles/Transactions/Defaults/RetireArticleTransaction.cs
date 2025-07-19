using Cashregister.Application.Articles.Commands;
using Cashregister.Application.Articles.Queries;
using Cashregister.Application.Common.Problems;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions.Defaults;

public sealed class RetireArticleTransaction(
    IFetchArticleQuery fetchArticleQuery,
    ISaveArticleCommand saveArticleCommand,
    IUnitOfWork unitOfWork
) :
    Transaction<Identifier, Unit>(unitOfWork),
    IRetireArticleTransaction
{
    protected override async Task<Result<Unit>> InternalExecuteAsync(Identifier articleId)
    {
        var maybeArticle = await fetchArticleQuery.FetchAsync(articleId);

        if (maybeArticle is null)
        {
            return Result.Error(new NoSuchThingProblem(articleId));
        }

        var retiredArticle = maybeArticle.Retire();

        await saveArticleCommand.SaveAsync(retiredArticle);

        return Result.Void();
    }
}