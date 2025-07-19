using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Input;

public sealed class ArticleDefinition
{
    public required string Description { get; init; }

    public required Cents Price { get; init; }
}