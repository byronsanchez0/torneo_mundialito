using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Application.Features.Matches.Commands;

public sealed record UpdateMatchCommand(
    Guid Id,
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime ScheduledAtUtc);

public sealed class UpdateMatchCommandHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        UpdateMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.HomeTeamId == Guid.Empty || command.AwayTeamId == Guid.Empty)
        {
            return Result.Failure(MatchErrors.TeamsAreRequired);
        }

        if (command.HomeTeamId == command.AwayTeamId)
        {
            return Result.Failure(MatchErrors.TeamsMustBeDifferent);
        }

        if (command.ScheduledAtUtc == default)
        {
            return Result.Failure(MatchErrors.InvalidScheduledDate);
        }

        var match = await matchRepository.GetByIdAsync(command.Id, cancellationToken);
        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound(command.Id));
        }

        if (match.Status != MatchStatus.Scheduled)
        {
            return Result.Failure(MatchErrors.OnlyScheduledCanBeUpdated(command.Id));
        }

        if (await teamRepository.GetByIdAsync(command.HomeTeamId, cancellationToken) is null)
        {
            return Result.Failure(MatchErrors.TeamNotFound(command.HomeTeamId));
        }

        if (await teamRepository.GetByIdAsync(command.AwayTeamId, cancellationToken) is null)
        {
            return Result.Failure(MatchErrors.TeamNotFound(command.AwayTeamId));
        }

        match.Update(command.HomeTeamId, command.AwayTeamId, command.ScheduledAtUtc);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
