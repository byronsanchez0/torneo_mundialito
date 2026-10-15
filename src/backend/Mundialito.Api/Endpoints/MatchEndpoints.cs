using Mundialito.Application.Features.Matches.Commands;
using Mundialito.Application.Features.Matches.Queries;
using Mundialito.Api.Idempotency;

namespace Mundialito.Api.Endpoints;

internal static class MatchEndpoints
{
    public static IEndpointRouteBuilder MapMatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/matches").WithTags("Matches");

        group.MapGet("", async (
            GetMatchesQueryHandler handler,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await handler.HandleAsync(
                new GetMatchesQuery(page, pageSize),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetMatchByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetMatchByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttpFailure();
        });

        group.MapPost("", async (
            CreateMatchRequest request,
            CreateMatchCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateMatchCommand(
                request.HomeTeamId,
                request.AwayTeamId,
                request.ScheduledAtUtc);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess
                ? Results.Json(new { id = result.Value }, statusCode: StatusCodes.Status201Created)
                : result.ToHttpFailure();
        }).AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMatchRequest request,
            UpdateMatchCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateMatchCommand(
                id,
                request.HomeTeamId,
                request.AwayTeamId,
                request.ScheduledAtUtc);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.Ok() : result.ToHttpFailure();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            DeleteMatchCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteMatchCommand(id), cancellationToken);
            return result.IsSuccess ? Results.Ok() : result.ToHttpFailure();
        });

        group.MapPost("/{matchId:guid}/goals", async (
            Guid matchId,
            RegisterGoalRequest request,
            RegisterGoalCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterGoalCommand(
                matchId,
                request.PlayerId,
                request.TeamId,
                request.Minute);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess
                ? Results.Json(new { id = result.Value }, statusCode: StatusCodes.Status201Created)
                : result.ToHttpFailure();
        }).AddEndpointFilter<IdempotencyEndpointFilter>();

        return endpoints;
    }

    private sealed record CreateMatchRequest(
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime ScheduledAtUtc);

    private sealed record UpdateMatchRequest(
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime ScheduledAtUtc);

    private sealed record RegisterGoalRequest(Guid PlayerId, Guid TeamId, int Minute);
}
