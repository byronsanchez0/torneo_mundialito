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
        GetPlayersQuery query,
        CancellationToken cancellationToken = default);

    Task<PlayerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record GetPlayersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "lastName",
    string SortDirection = "asc",
    Guid? TeamId = null,
    string? Name = null,
    string? Position = null,
    int? ShirtNumber = null);

public sealed record GetPlayerByIdQuery(Guid Id);

public sealed class GetPlayersQueryHandler(IPlayerQueries playerQueries)
{
    public Task<PagedResult<PlayerDto>> HandleAsync(
        GetPlayersQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query with
        {
            PageNumber = Math.Max(query.PageNumber, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            SortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "lastName" : query.SortBy.Trim(),
            SortDirection = NormalizeSortDirection(query.SortDirection),
            Name = NormalizeFilter(query.Name),
            Position = NormalizeFilter(query.Position)
        };

        return playerQueries.GetPagedAsync(normalizedQuery, cancellationToken);
    }

    private static string NormalizeSortDirection(string sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
