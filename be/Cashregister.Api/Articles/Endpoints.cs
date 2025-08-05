namespace Cashregister.Api.Articles;

internal static class Endpoints
{
    public static RouteGroupBuilder MapArticles(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/articles");

        routeGroup.MapGet("/", Handlers.GetArticlesPage);
        routeGroup.MapPost("/", Handlers.RegisterArticle);
        routeGroup.MapGet("/{id}", Handlers.GetArticle);

        return routeGroup;
    }
}