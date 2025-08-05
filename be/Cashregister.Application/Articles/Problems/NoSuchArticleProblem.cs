using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Articles.Problems;

public sealed record NoSuchArticleProblem(Identifier MissingArticle) : Problem;