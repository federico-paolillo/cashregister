using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Cashregister.Domain;

namespace Cashregister.Application.Pagination;

public sealed class Page<T> where T : class, IPageItem
{
    [MemberNotNullWhen(true, nameof(Next))]
    public required bool HasNext { get; init; }

    public required Identifier? Next { get; init; }

    public required ImmutableArray<T> Items { get; init; }

    public int Size => Items.Length;
}