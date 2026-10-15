using Mundialito.Application.Features.Players.Commands;

namespace Mundialito.Api.Endpoints;

internal static class PlayerEndpoints
{
    public static IEndpointRouteBuilder MapPlayerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/players").WithTags("Players");

        group.MapPost("", async (
            CreatePlayerRequest request,
            CreatePlayerCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePlayerCommand(
                request.TeamId,
                request.FirstName,
                request.LastName,
                request.ShirtNumber,
                request.Position);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/players/{result.Value}", new { id = result.Value })
                : result.ToHttpFailure();
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdatePlayerRequest request,
            UpdatePlayerCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePlayerCommand(
                id,
                request.TeamId,
                request.FirstName,
                request.LastName,
                request.ShirtNumber,
                request.Position);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToHttpFailure();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            DeletePlayerCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeletePlayerCommand(id), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToHttpFailure();
        });

        return endpoints;
    }

    private sealed record CreatePlayerRequest(
        Guid TeamId,
        string FirstName,
        string LastName,
        int ShirtNumber,
        string Position);

    private sealed record UpdatePlayerRequest(
        Guid TeamId,
        string FirstName,
        string LastName,
        int ShirtNumber,
        string Position);
}
