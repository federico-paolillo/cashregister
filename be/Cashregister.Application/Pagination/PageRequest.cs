using Cashregister.Domain;

namespace Cashregister.Application.Pagination;

public sealed class PageRequest
{
    public required Identifier? After { get; init; }

    public Identifier? Until { get; init; }

    public required uint Size { get; init; }
}
