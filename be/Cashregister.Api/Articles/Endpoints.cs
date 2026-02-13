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

        routeGroup.MapPost("/{id}", Handlers.ChangeArticle)
            .WithName("ChangeArticle");

        routeGroup.MapDelete("/{id}", Handlers.DeleteArticle)
            .WithName("RetireArticle");

        return routeGroup;
    }
}