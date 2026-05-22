using System.Linq.Expressions;

using Cashregister.Application.Articles.Models.Output;
using Cashregister.Database.Entities;
using Cashregister.Domain;

namespace Cashregister.Database.Queries;

public sealed class FetchArticlesListQuery(
    IApplicationDbContext applicationDbContext
) : PaginationQuery<ArticleEntity, ArticleListItem>
{
    protected override IQueryable<ArticleEntity> GetQueryable()
    {
        return applicationDbContext.Articles;
    }

    protected override Expression<Func<ArticleEntity, ArticleListItem>> GetProjection()
    {
        return a => new ArticleListItem
        {
            Id = Identifier.From(a.Id),
            Description = a.Description,
            Price = Cents.From(a.Price),
            QuantityAvailable = a.QuantityAvailable
        };
    }
}