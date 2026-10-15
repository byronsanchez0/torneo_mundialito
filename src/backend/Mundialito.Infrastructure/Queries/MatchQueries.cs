using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Matches.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class MatchQueries(NpgsqlDataSource dataSource) : IMatchQueries
{
    private static readonly IReadOnlyDictionary<string, string> SortColumns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["scheduledAtUtc"] = "m.\"ScheduledAtUtc\"",
            ["status"] = "m.\"Status\"",
            ["homeTeamName"] = "home.\"Name\"",
            ["awayTeamName"] = "away.\"Name\"",
            ["homeScore"] = "\"HomeScore\"",
            ["awayScore"] = "\"AwayScore\""
        };

    private const string Filters = """
        WHERE (
            CAST(@TeamId AS uuid) IS NULL
            OR m."HomeTeamId" = @TeamId
            OR m."AwayTeamId" = @TeamId
        )
          AND (
              CAST(@Status AS text) IS NULL
              OR LOWER(m."Status") = LOWER(@Status)
          )
          AND (
              CAST(@FromDate AS timestamp with time zone) IS NULL
              OR m."ScheduledAtUtc" >= @FromDate
          )
          AND (
              CAST(@ToDate AS timestamp with time zone) IS NULL
              OR m."ScheduledAtUtc" <= @ToDate
          )
        """;

    private const string MatchProjection = """
        SELECT
            m."Id" AS "Id",
            m."HomeTeamId" AS "HomeTeamId",
            home."Name" AS "HomeTeamName",
            m."AwayTeamId" AS "AwayTeamId",
            away."Name" AS "AwayTeamName",
            m."ScheduledAtUtc" AS "ScheduledAtUtc",
            m."Status" AS "Status",
            (
                SELECT COUNT(*)::int
                FROM goals g
                WHERE g."MatchId" = m."Id" AND g."TeamId" = m."HomeTeamId"
            ) AS "HomeScore",
            (
                SELECT COUNT(*)::int
                FROM goals g
                WHERE g."MatchId" = m."Id" AND g."TeamId" = m."AwayTeamId"
            ) AS "AwayScore"
        FROM matches m
        INNER JOIN teams home ON home."Id" = m."HomeTeamId"
        INNER JOIN teams away ON away."Id" = m."AwayTeamId"
        """;

    public async Task<PagedResult<MatchDto>> GetPagedAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var orderBy = GetOrderBy(query.SortBy, query.SortDirection);
        var sql = $$"""
            SELECT COUNT(*)
            FROM matches m
            {{Filters}};

            {{MatchProjection}}
            {{Filters}}
            ORDER BY {{orderBy}}
            LIMIT @PageSize OFFSET @Offset;
            """;

        var parameters = new
        {
            query.TeamId,
            query.Status,
            query.FromDate,
            query.ToDate,
            query.PageSize,
            Offset = (long)(query.PageNumber - 1) * query.PageSize
        };

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);

        var totalRecords = await results.ReadSingleAsync<long>();
        var data = (await results.ReadAsync<MatchDto>()).ToList();

        return new PagedResult<MatchDto>(
            data,
            query.PageNumber,
            query.PageSize,
            totalRecords);
    }

    public async Task<MatchDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sql = $$"""
            {{MatchProjection}}
            WHERE m."Id" = @Id;

            SELECT
                g."Id" AS "Id",
                g."PlayerId" AS "PlayerId",
                CONCAT(p."FirstName", ' ', p."LastName") AS "PlayerName",
                g."TeamId" AS "TeamId",
                g."Minute" AS "Minute"
            FROM goals g
            INNER JOIN players p ON p."Id" = g."PlayerId"
            WHERE g."MatchId" = @Id
            ORDER BY g."Minute", g."Id";
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);

        var match = await results.ReadSingleOrDefaultAsync<MatchDto>();
        var goals = (await results.ReadAsync<GoalDto>()).ToList();

        return match is null
            ? null
            : new MatchDetailsDto(
                match.Id,
                match.HomeTeamId,
                match.HomeTeamName,
                match.AwayTeamId,
                match.AwayTeamName,
                match.ScheduledAtUtc,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                goals);
    }

    private static string GetOrderBy(string sortBy, string sortDirection)
    {
        if (!SortColumns.TryGetValue(sortBy, out var column))
        {
            column = SortColumns["scheduledAtUtc"];
        }

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        return $"{column} {direction}, m.\"Id\"";
    }
}
