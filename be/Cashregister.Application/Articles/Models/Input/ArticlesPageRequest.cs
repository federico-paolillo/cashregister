using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Input;

public sealed class ArticlesPageRequest
{
    public required Identifier? After { get; init; }

    public Identifier? Until { get; init; }

    public required uint Size { get; init; }
}