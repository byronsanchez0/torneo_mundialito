using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;

namespace Mundialito.Application.Features.Teams.Commands;

public sealed record DeleteTeamCommand(Guid Id);

public sealed class DeleteTeamCommandHandler(
    ITeamRepository teamRepository,
    IPlayerRepository playerRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        DeleteTeamCommand command,
        CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetByIdAsync(command.Id, cancellationToken);
        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound(command.Id));
        }

        var hasPlayers = await playerRepository.AnyByTeamIdAsync(command.Id, cancellationToken);
        var hasMatches = await matchRepository.AnyByTeamIdAsync(command.Id, cancellationToken);
        if (hasPlayers || hasMatches)
        {
            return Result.Failure(TeamErrors.HasRelatedRecords(command.Id));
        }

        teamRepository.Remove(team);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
