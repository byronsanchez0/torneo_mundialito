using Mundialito.Application.Features.Teams.Commands;
using Mundialito.Application.Features.Teams.Queries;

namespace Mundialito.Api.Endpoints;

internal static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/teams").WithTags("Teams");

        group.MapGet("", async (
            GetTeamsQueryHandler handler,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await handler.HandleAsync(
                new GetTeamsQuery(page, pageSize),
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
                ? Results.Created($"/api/teams/{result.Value}", new { id = result.Value })
                : result.ToHttpFailure();
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTeamRequest request,
            UpdateTeamCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new UpdateTeamCommand(id, request.Name, request.Coach),
                cancellationToken);

            return result.IsSuccess ? Results.NoContent() : result.ToHttpFailure();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            DeleteTeamCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteTeamCommand(id), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToHttpFailure();
        });

        return endpoints;
    }

    private sealed record CreateTeamRequest(string Name, string? Coach);
    private sealed record UpdateTeamRequest(string Name, string? Coach);
}
