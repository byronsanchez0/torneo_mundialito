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
        GetMatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<MatchDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetMatchesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "scheduledAtUtc",
    string SortDirection = "desc",
    Guid? TeamId = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

public sealed record GetMatchByIdQuery(Guid Id);

public sealed class GetMatchesQueryHandler(IMatchQueries matchQueries)
{
    public Task<PagedResult<MatchDto>> HandleAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query with
        {
            PageNumber = Math.Max(query.PageNumber, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            SortBy = string.IsNullOrWhiteSpace(query.SortBy)
                ? "scheduledAtUtc"
                : query.SortBy.Trim(),
            SortDirection = NormalizeSortDirection(query.SortDirection),
            Status = NormalizeFilter(query.Status),
            FromDate = NormalizeDate(query.FromDate),
            ToDate = NormalizeDate(query.ToDate)
        };

        return matchQueries.GetPagedAsync(normalizedQuery, cancellationToken);
    }

    private static string NormalizeSortDirection(string sortDirection) =>
        string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime? NormalizeDate(DateTime? value) =>
        value?.ToUniversalTime();
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
