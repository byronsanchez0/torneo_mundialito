using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Application.Features.Teams.Commands;

public sealed record CreateTeamCommand(string Name, string? Coach);

public sealed class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> HandleAsync(
        CreateTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<Guid>.Failure(TeamErrors.NameIsRequired);
        }

        var name = command.Name.Trim();
        if (await teamRepository.NameExistsAsync(name, cancellationToken: cancellationToken))
        {
            return Result<Guid>.Failure(TeamErrors.NameAlreadyExist(name));
        }

        var team = new Team(name, command.Coach);
        teamRepository.Add(team);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(team.Id);
    }
}
