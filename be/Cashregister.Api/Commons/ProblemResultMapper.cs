using Cashregister.Application.Articles.Problems;
using Cashregister.Application.Orders.Problems;
using Cashregister.Factories;

namespace Cashregister.Api.Commons;

internal static class ProblemResultMapper
{
    public static IResult ToHttpResult(this Problem problem) => problem switch
    {
        NoSuchArticleProblem => TypedResults.NotFound(),
        OrderRequestIsMissingSomeArticles => TypedResults.BadRequest(),
        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
    };
}
