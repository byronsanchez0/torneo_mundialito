using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Application.Features.Players.Commands;

public sealed record CreatePlayerCommand(
    Guid TeamId,
    string FirstName,
    string LastName,
    int ShirtNumber,
    string Position);

public sealed class CreatePlayerCommandHandler(
    IPlayerRepository playerRepository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> HandleAsync(
        CreatePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command.FirstName, command.LastName, command.Position, command.ShirtNumber);
        if (validationError is not null)
        {
            return Result<Guid>.Failure(validationError);
        }

        if (await teamRepository.GetByIdAsync(command.TeamId, cancellationToken) is null)
        {
            return Result<Guid>.Failure(PlayerErrors.TeamNotFound(command.TeamId));
        }

        if (await playerRepository.ShirtNumberExistsAsync(
                command.TeamId,
                command.ShirtNumber,
                cancellationToken: cancellationToken))
        {
            return Result<Guid>.Failure(
                PlayerErrors.ShirtNumberAlreadyExist(command.TeamId, command.ShirtNumber));
        }

        var player = new Player(
            command.TeamId,
            command.FirstName,
            command.LastName,
            command.ShirtNumber,
            command.Position);

        playerRepository.Add(player);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(player.Id);
    }

    private static Error? Validate(string firstName, string lastName, string position, int shirtNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName)
            || string.IsNullOrWhiteSpace(position))
        {
            return PlayerErrors.RequiredFields;
        }

        return shirtNumber is < 1 or > 99 ? PlayerErrors.InvalidShirtNumber : null;
    }
}
