using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Input;

public sealed class ArticleChange
{
    public required Identifier Id { get; init; }

    public required string Description { get; init; }

    public required Cents Price { get; init; }
}