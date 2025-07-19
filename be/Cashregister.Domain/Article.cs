namespace Cashregister.Domain;

public sealed class Article
{
    public required Identifier Id { get; init; }

    public required string Description { get; init; }

    public required Cents Price { get; init; }

    public RetiredArticle Retire()
    {
        return new RetiredArticle
        {
            Id = Id,
        };
    }
}