using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Application.Features.Matches.Commands;

public sealed record CreateMatchCommand(
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime ScheduledAtUtc);

public sealed class CreateMatchCommandHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> HandleAsync(
        CreateMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        if (validationError is not null)
        {
            return Result<Guid>.Failure(validationError);
        }

        if (await teamRepository.GetByIdAsync(command.HomeTeamId, cancellationToken) is null)
        {
            return Result<Guid>.Failure(MatchErrors.TeamNotFound(command.HomeTeamId));
        }

        if (await teamRepository.GetByIdAsync(command.AwayTeamId, cancellationToken) is null)
        {
            return Result<Guid>.Failure(MatchErrors.TeamNotFound(command.AwayTeamId));
        }

        var match = new Match(command.HomeTeamId, command.AwayTeamId, command.ScheduledAtUtc);
        matchRepository.Add(match);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(match.Id);
    }

    private static Error? Validate(CreateMatchCommand command)
    {
        if (command.HomeTeamId == Guid.Empty || command.AwayTeamId == Guid.Empty)
        {
            return MatchErrors.TeamsAreRequired;
        }

        if (command.HomeTeamId == command.AwayTeamId)
        {
            return MatchErrors.TeamsMustBeDifferent;
        }

        return command.ScheduledAtUtc == default ? MatchErrors.InvalidScheduledDate : null;
    }
}
