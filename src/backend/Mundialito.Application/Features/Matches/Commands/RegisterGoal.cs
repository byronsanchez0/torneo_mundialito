using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Application.Features.Matches.Commands;

public sealed record RegisterGoalCommand(
    Guid MatchId,
    Guid PlayerId,
    Guid TeamId,
    int Minute);

public sealed class RegisterGoalCommandHandler(
    IMatchRepository matchRepository,
    IPlayerRepository playerRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> HandleAsync(
        RegisterGoalCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Minute is < 1 or > 120)
        {
            return Result<Guid>.Failure(MatchErrors.InvalidGoalMinute);
        }

        var match = await matchRepository.GetByIdAsync(command.MatchId, cancellationToken);
        if (match is null)
        {
            return Result<Guid>.Failure(MatchErrors.NotFound(command.MatchId));
        }

        if (match.Status == MatchStatus.Finished)
        {
            return Result<Guid>.Failure(MatchErrors.AlreadyFinished(command.MatchId));
        }

        if (command.TeamId != match.HomeTeamId && command.TeamId != match.AwayTeamId)
        {
            return Result<Guid>.Failure(
                MatchErrors.TeamDoesNotParticipate(command.MatchId, command.TeamId));
        }

        var player = await playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<Guid>.Failure(MatchErrors.PlayerNotFound(command.PlayerId));
        }

        if (player.TeamId != command.TeamId)
        {
            return Result<Guid>.Failure(
                MatchErrors.PlayerDoesNotBelongToTeam(command.PlayerId, command.TeamId));
        }

        Goal goal = match.AddGoal(command.PlayerId, command.TeamId, command.Minute);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(goal.Id);
    }
}
