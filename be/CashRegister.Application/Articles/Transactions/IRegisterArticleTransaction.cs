using Cashregister.Application.Models.Input;
using Cashregister.Domain;

namespace Cashregister.Application.Articles.Transactions;

public interface IRegisterArticleTransaction
{
    Task<Identifier> RegisterArticleAsync(ArticleDefinition articleDefinition);
}