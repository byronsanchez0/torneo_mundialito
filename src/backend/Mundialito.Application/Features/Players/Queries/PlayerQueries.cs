using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Players.Queries;

public sealed record PlayerDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string FirstName,
    string LastName,
    int ShirtNumber,
    string Position);

public interface IPlayerQueries
{
    Task<PagedResult<PlayerDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? teamId,
        CancellationToken cancellationToken = default);

    Task<PlayerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetPlayersQuery(int Page = 1, int PageSize = 20, Guid? TeamId = null);
public sealed record GetPlayerByIdQuery(Guid Id);

public sealed class GetPlayersQueryHandler(IPlayerQueries playerQueries)
{
    public Task<PagedResult<PlayerDto>> HandleAsync(
        GetPlayersQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        return playerQueries.GetPagedAsync(page, pageSize, query.TeamId, cancellationToken);
    }
}

public sealed class GetPlayerByIdQueryHandler(IPlayerQueries playerQueries)
{
    public async Task<Result<PlayerDto>> HandleAsync(
        GetPlayerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var player = await playerQueries.GetByIdAsync(query.Id, cancellationToken);
        return player is null
            ? Result<PlayerDto>.Failure(PlayerErrors.NotFound(query.Id))
            : Result<PlayerDto>.Success(player);
    }
}
