using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Cashregister.Domain;

namespace Cashregister.Application.Articles.Models.Output;

public sealed class ArticlesPage
{
    [MemberNotNullWhen(true, nameof(Next))]
    public required bool HasNext { get; init; }
    
    public required Identifier? Next { get; init; }
    
    public required ImmutableArray<ArticleListItem> Articles { get; init; }

    public int Size => Articles.Length;
}