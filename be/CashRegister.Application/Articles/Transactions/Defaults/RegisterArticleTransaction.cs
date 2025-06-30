using Cashregister.Application.Models.Input;
using Cashregister.Domain;

namespace Cashregister.Application.Articles.Transactions.Defaults;

public sealed class RegisterArticleTransaction(ISaveArticleCommand) : IRegisterArticleTransaction
{
    public Task<Identifier> RegisterArticleAsync(ArticleDefinition articleDefinition)
    {
        ArgumentNullException.ThrowIfNull(articleDefinition); 
        
        var article = new Article
        {
            Id = Identifier.New(),
            Description = articleDefinition.Description,
            Price = articleDefinition.Price,
        };

        await saveArticleCommand.SaveAsync(article);
    }
}