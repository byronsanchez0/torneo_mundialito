using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Matches.Queries;

public sealed record MatchDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime ScheduledAtUtc,
    string Status,
    int HomeScore,
    int AwayScore);

public sealed record GoalDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid TeamId,
    int Minute);

public sealed record MatchDetailsDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime ScheduledAtUtc,
    string Status,
    int HomeScore,
    int AwayScore,
    IReadOnlyCollection<GoalDto> Goals);

public interface IMatchQueries
{
    Task<PagedResult<MatchDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<MatchDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetMatchesQuery(int Page = 1, int PageSize = 20);
public sealed record GetMatchByIdQuery(Guid Id);

public sealed class GetMatchesQueryHandler(IMatchQueries matchQueries)
{
    public Task<PagedResult<MatchDto>> HandleAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        return matchQueries.GetPagedAsync(page, pageSize, cancellationToken);
    }
}

public sealed class GetMatchByIdQueryHandler(IMatchQueries matchQueries)
{
    public async Task<Result<MatchDetailsDto>> HandleAsync(
        GetMatchByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var match = await matchQueries.GetByIdAsync(query.Id, cancellationToken);
        return match is null
            ? Result<MatchDetailsDto>.Failure(MatchErrors.NotFound(query.Id))
            : Result<MatchDetailsDto>.Success(match);
    }
}
