namespace Cashregister.Api.ReceiptModes;

internal static class Endpoints
{
    public static RouteGroupBuilder MapReceiptModes(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/receipt-mode");

        routeGroup.MapGet("/", Handlers.GetReceiptMode)
            .WithName("GetReceiptMode");

        routeGroup.MapPost("/{mode}", Handlers.SelectReceiptMode)
            .WithName("SelectReceiptMode");

        return routeGroup;
    }
}