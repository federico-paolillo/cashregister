namespace Cashregister.Api.Articles;

internal static class Endpoints
{
    public static RouteGroupBuilder MapArticles(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/articles");

        routeGroup.MapGet("/", Handlers.GetArticlesPage)
            .WithName("GetArticlesPage");

        routeGroup.MapPost("/", Handlers.RegisterArticle)
            .WithName("RegisterArticle");

        routeGroup.MapGet("/{id}", Handlers.GetArticle)
            .WithName("GetArticle");

        routeGroup.MapDelete("/{id}", Handlers.DeleteArticle)
            .WithName("RetireArticle");

        return routeGroup;
    }
}