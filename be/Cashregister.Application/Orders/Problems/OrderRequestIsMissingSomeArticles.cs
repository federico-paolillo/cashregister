using System.Collections.Immutable;

using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Orders.Problems;

public sealed record OrderRequestIsMissingSomeArticles(ImmutableArray<Identifier> ArticlesRequested) : Problem;