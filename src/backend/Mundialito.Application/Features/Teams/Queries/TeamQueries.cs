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
        GetTeamsQuery query,
        CancellationToken cancellationToken = default);

    Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetTeamsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "name",
    string SortDirection = "asc",
    string? Name = null,
    string? Coach = null);

public sealed record GetTeamByIdQuery(Guid Id);

public sealed class GetTeamsQueryHandler(ITeamQueries teamQueries)
{
    public Task<PagedResult<TeamDto>> HandleAsync(
        GetTeamsQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query with
        {
            PageNumber = Math.Max(query.PageNumber, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            SortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "name" : query.SortBy.Trim(),
            SortDirection = NormalizeSortDirection(query.SortDirection),
            Name = NormalizeFilter(query.Name),
            Coach = NormalizeFilter(query.Coach)
        };

        return teamQueries.GetPagedAsync(normalizedQuery, cancellationToken);
    }

    private static string NormalizeSortDirection(string sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
