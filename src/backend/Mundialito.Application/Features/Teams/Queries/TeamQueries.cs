using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Teams.Queries;

public sealed record TeamDto(
    Guid Id,
    string Name,
    string? Coach,
    int PlayerCount,
    int MatchCount);

public interface ITeamQueries
{
    Task<PagedResult<TeamDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetTeamsQuery(int Page = 1, int PageSize = 20);
public sealed record GetTeamByIdQuery(Guid Id);

public sealed class GetTeamsQueryHandler(ITeamQueries teamQueries)
{
    public Task<PagedResult<TeamDto>> HandleAsync(
        GetTeamsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        return teamQueries.GetPagedAsync(page, pageSize, cancellationToken);
    }
}

public sealed class GetTeamByIdQueryHandler(ITeamQueries teamQueries)
{
    public async Task<Result<TeamDto>> HandleAsync(
        GetTeamByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var team = await teamQueries.GetByIdAsync(query.Id, cancellationToken);
        return team is null
            ? Result<TeamDto>.Failure(TeamErrors.NotFound(query.Id))
            : Result<TeamDto>.Success(team);
    }
}
