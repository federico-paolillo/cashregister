namespace Cashregister.Api.Statistics;

/// <summary>
/// Maps statistics HTTP endpoints.
/// </summary>
internal static class Endpoints
{
    public static RouteGroupBuilder MapStatistics(this WebApplication webApplication)
    {
        var routeGroup = webApplication.MapGroup("/statistics");

        routeGroup.MapGet("/", Handlers.GetStatistics)
            .WithName("GetStatistics");

        routeGroup.MapGet("/sales.csv", Handlers.GetSalesStatisticsCsv)
            .WithName("GetSalesStatisticsCsv");

        return routeGroup;
    }
}