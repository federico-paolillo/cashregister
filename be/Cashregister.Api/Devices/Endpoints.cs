namespace Cashregister.Api.Devices;

internal static class Endpoints
{
    public static RouteGroupBuilder MapDevices(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/devices");

        routeGroup.MapGet("/", Handlers.GetDevices)
            .WithName("GetDevices");

        routeGroup.MapPost("/{id}", Handlers.SelectDevice)
            .WithName("SelectDevice");

        return routeGroup;
    }
}