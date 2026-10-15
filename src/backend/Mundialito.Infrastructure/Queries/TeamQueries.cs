using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Teams.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class TeamQueries(NpgsqlDataSource dataSource) : ITeamQueries
{
    public async Task<PagedResult<TeamDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM teams;

            SELECT
                t."Id" AS "Id",
                t."Name" AS "Name",
                t."Coach" AS "Coach",
                (
                    SELECT COUNT(*)::int
                    FROM players p
                    WHERE p."TeamId" = t."Id"
                ) AS "PlayerCount",
                (
                    SELECT COUNT(*)::int
                    FROM matches m
                    WHERE m."HomeTeamId" = t."Id" OR m."AwayTeamId" = t."Id"
                ) AS "MatchCount"
            FROM teams t
            ORDER BY t."Name", t."Id"
            LIMIT @PageSize OFFSET @Offset;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { PageSize = pageSize, Offset = (long)(page - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var totalCount = await results.ReadSingleAsync<long>();
        var items = (await results.ReadAsync<TeamDto>()).ToList();

        return new PagedResult<TeamDto>(items, page, pageSize, totalCount);
    }

    public async Task<TeamDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                t."Id" AS "Id",
                t."Name" AS "Name",
                t."Coach" AS "Coach",
                (
                    SELECT COUNT(*)::int
                    FROM players p
                    WHERE p."TeamId" = t."Id"
                ) AS "PlayerCount",
                (
                    SELECT COUNT(*)::int
                    FROM matches m
                    WHERE m."HomeTeamId" = t."Id" OR m."AwayTeamId" = t."Id"
                ) AS "MatchCount"
            FROM teams t
            WHERE t."Id" = @Id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TeamDto>(command);
    }
}
