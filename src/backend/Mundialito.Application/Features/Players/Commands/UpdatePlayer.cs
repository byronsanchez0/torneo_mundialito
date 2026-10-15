using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;

namespace Mundialito.Application.Features.Players.Commands;

public sealed record UpdatePlayerCommand(
    Guid Id,
    Guid TeamId,
    string FirstName,
    string LastName,
    int ShirtNumber,
    string Position);

public sealed class UpdatePlayerCommandHandler(
    IPlayerRepository playerRepository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        UpdatePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName)
            || string.IsNullOrWhiteSpace(command.LastName)
            || string.IsNullOrWhiteSpace(command.Position))
        {
            return Result.Failure(PlayerErrors.RequiredFields);
        }

        if (command.ShirtNumber is < 1 or > 99)
        {
            return Result.Failure(PlayerErrors.InvalidShirtNumber);
        }

        var player = await playerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (player is null)
        {
            return Result.Failure(PlayerErrors.NotFound(command.Id));
        }

        if (await teamRepository.GetByIdAsync(command.TeamId, cancellationToken) is null)
        {
            return Result.Failure(PlayerErrors.TeamNotFound(command.TeamId));
        }

        if (await playerRepository.ShirtNumberExistsAsync(
                command.TeamId,
                command.ShirtNumber,
                command.Id,
                cancellationToken))
        {
            return Result.Failure(
                PlayerErrors.ShirtNumberAlreadyExist(command.TeamId, command.ShirtNumber));
        }

        player.Update(
            command.TeamId,
            command.FirstName,
            command.LastName,
            command.ShirtNumber,
            command.Position);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
