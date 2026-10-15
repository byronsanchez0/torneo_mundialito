using Mundialito.Application.Features.Teams.Commands;
using Mundialito.Application.Features.Teams.Queries;
using Mundialito.Api.Idempotency;

namespace Mundialito.Api.Endpoints;

internal static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/teams").WithTags("Teams");

        group.MapGet("", async (
            GetTeamsQueryHandler handler,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "name",
            string sortDirection = "asc",
            string? name = null,
            string? coach = null) =>
        {
            var result = await handler.HandleAsync(
                new GetTeamsQuery(
                    pageNumber,
                    pageSize,
                    sortBy,
                    sortDirection,
                    name,
                    coach),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetTeamByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetTeamByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttpFailure();
        });

        group.MapPost("", async (
            CreateTeamRequest request,
            CreateTeamCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new CreateTeamCommand(request.Name, request.Coach),
                cancellationToken);

            return result.IsSuccess
                ? Results.Json(new { id = result.Value }, statusCode: StatusCodes.Status201Created)
                : result.ToHttpFailure();
        }).AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTeamRequest request,
            UpdateTeamCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new UpdateTeamCommand(id, request.Name, request.Coach),
                cancellationToken);

            return result.IsSuccess ? Results.Ok() : result.ToHttpFailure();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            DeleteTeamCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteTeamCommand(id), cancellationToken);
            return result.IsSuccess ? Results.Ok() : result.ToHttpFailure();
        });

        return endpoints;
    }

    private sealed record CreateTeamRequest(string Name, string? Coach);
    private sealed record UpdateTeamRequest(string Name, string? Coach);
}
