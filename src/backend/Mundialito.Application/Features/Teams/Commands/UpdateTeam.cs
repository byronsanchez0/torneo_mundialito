using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;

namespace Mundialito.Application.Features.Teams.Commands;

public sealed record UpdateTeamCommand(Guid Id, string Name, string? Coach);

public sealed class UpdateTeamCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        UpdateTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure(TeamErrors.NameIsRequired);
        }

        var team = await teamRepository.GetByIdAsync(command.Id, cancellationToken);
        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound(command.Id));
        }

        var name = command.Name.Trim();
        if (await teamRepository.NameExistsAsync(name, command.Id, cancellationToken))
        {
            return Result.Failure(TeamErrors.NameAlreadyExist(name));
        }

        team.Rename(name);
        team.ChangeCoach(command.Coach);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
