using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;

namespace Mundialito.Application.Features.Players.Commands;

public sealed record DeletePlayerCommand(Guid Id);

public sealed class DeletePlayerCommandHandler(
    IPlayerRepository playerRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        DeletePlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        var player = await playerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (player is null)
        {
            return Result.Failure(PlayerErrors.NotFound(command.Id));
        }

        if (await matchRepository.AnyGoalByPlayerIdAsync(command.Id, cancellationToken))
        {
            return Result.Failure(PlayerErrors.HasRegisteredGoals(command.Id));
        }

        playerRepository.Remove(player);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
