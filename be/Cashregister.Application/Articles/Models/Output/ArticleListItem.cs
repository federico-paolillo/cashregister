using Cashregister.Application.Pagination;
using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Output;

public sealed class ArticleListItem : IPageItem
{
    public required string Description { get; init; }

    public required Cents Price { get; init; }

    public required Identifier Id { get; init; }

    public long? QuantityAvailable { get; init; }
}