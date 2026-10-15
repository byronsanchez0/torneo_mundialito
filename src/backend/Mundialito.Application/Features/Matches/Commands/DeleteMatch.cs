using Mundialito.Application.Common.Results;
using Mundialito.Application.Contracts;

namespace Mundialito.Application.Features.Matches.Commands;

public sealed record DeleteMatchCommand(Guid Id);

public sealed class DeleteMatchCommandHandler(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        DeleteMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var match = await matchRepository.GetByIdAsync(command.Id, cancellationToken);
        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound(command.Id));
        }

        matchRepository.Remove(match);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
