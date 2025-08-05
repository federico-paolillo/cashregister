using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Application.Articles.Problems;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions.Defaults;

public sealed class ChangeArticleTransaction(
    IUnitOfWork unitOfWork,
    IFetchArticleQuery fetchArticleQuery,
    ISaveArticleCommand saveArticleCommand
) :
  Transaction<ArticleChange, Unit>(unitOfWork),
  IChangeArticleTransaction
{
  protected override async Task<Result<Unit>> InternalExecuteAsync(ArticleChange change)
  {
    ArgumentNullException.ThrowIfNull(change);

    var maybeArticleToChange = await fetchArticleQuery.FetchAsync(change.Id);

    if (maybeArticleToChange is null)
    {
      return Result.Error<Unit>(new NoSuchArticleProblem(change.Id));
    }

    var updatedArticle = new Article
    {
      Id = change.Id,
      Description = change.Description,
      Price = change.Price
    };

    await saveArticleCommand.SaveAsync(updatedArticle);

    return Result.Void();
  }
}