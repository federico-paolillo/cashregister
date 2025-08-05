using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Output;

public sealed class ArticleListItem
{
    public required Identifier Id { get; init; }

    public required string Description { get; init; }

    public required Cents Price { get; init; }
}