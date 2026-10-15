using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Matches.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class MatchQueries(NpgsqlDataSource dataSource) : IMatchQueries
{
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
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var sql = $$"""
            SELECT COUNT(*)
            FROM matches;

            {{MatchProjection}}
            ORDER BY m."ScheduledAtUtc" DESC, m."Id"
            LIMIT @PageSize OFFSET @Offset;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { PageSize = pageSize, Offset = (long)(page - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var totalCount = await results.ReadSingleAsync<long>();
        var items = (await results.ReadAsync<MatchDto>()).ToList();

        return new PagedResult<MatchDto>(items, page, pageSize, totalCount);
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
}
