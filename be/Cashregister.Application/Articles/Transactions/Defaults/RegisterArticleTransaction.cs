using Cashregister.Application.Articles.Data;
using Cashregister.Application.Articles.Models.Input;
using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Transactions.Defaults;

public sealed class RegisterArticleTransaction(
    ISaveArticleCommand saveArticleCommand,
    IUnitOfWork unitOfWork
) :
    Transaction<ArticleDefinition, Identifier>(unitOfWork),
    IRegisterArticleTransaction
{
    protected override async Task<Result<Identifier>> InternalExecuteAsync(ArticleDefinition articleDefinition)
    {
        ArgumentNullException.ThrowIfNull(articleDefinition);

        var newArticleId = Identifier.New();

        var article = new Article
        {
            Id = newArticleId,
            Description = articleDefinition.Description,
            Price = articleDefinition.Price,
        };

        await saveArticleCommand.SaveAsync(article);

        return Result.Ok(newArticleId);
    }
}