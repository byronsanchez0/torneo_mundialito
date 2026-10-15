using Mundialito.Application.Features.Statistics.Queries;

namespace Mundialito.Api.Endpoints;

internal static class StatisticsEndpoints
{
    public static IEndpointRouteBuilder MapStatisticsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/statistics", async (
            GetTournamentStatisticsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var statistics = await handler.HandleAsync(cancellationToken);
            return Results.Ok(statistics);
        }).WithTags("Statistics");

        return endpoints;
    }
}
