namespace Cashregister.Api.Orders;

internal static class Endpoints
{
    public static RouteGroupBuilder MapOrders(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/orders");

        routeGroup.MapGet("/", Handlers.GetOrdersPage)
            .WithName("GetOrdersPage");

        routeGroup.MapPost("/", Handlers.CreateOrder)
            .WithName("CreateOrder");

        routeGroup.MapGet("/{id}", Handlers.GetOrder)
            .WithName("GetOrder");

        return routeGroup;
    }
}