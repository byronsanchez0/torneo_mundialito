using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Teams.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class TeamQueries(NpgsqlDataSource dataSource) : ITeamQueries
{
    private static readonly IReadOnlyDictionary<string, string> SortColumns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "t.\"Name\"",
            ["coach"] = "t.\"Coach\"",
            ["playerCount"] = "\"PlayerCount\"",
            ["matchCount"] = "\"MatchCount\""
        };

    private const string Filters = """
        WHERE (CAST(@Name AS text) IS NULL OR t."Name" ILIKE '%' || @Name || '%')
          AND (CAST(@Coach AS text) IS NULL OR t."Coach" ILIKE '%' || @Coach || '%')
        """;

    public async Task<PagedResult<TeamDto>> GetPagedAsync(
        GetTeamsQuery query,
        CancellationToken cancellationToken = default)
    {
        var orderBy = GetOrderBy(query.SortBy, query.SortDirection);
        var sql = $$"""
            SELECT COUNT(*)
            FROM teams t
            {{Filters}};

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
            {{Filters}}
            ORDER BY {{orderBy}}
            LIMIT @PageSize OFFSET @Offset;
            """;

        var parameters = new
        {
            query.Name,
            query.Coach,
            query.PageSize,
            Offset = (long)(query.PageNumber - 1) * query.PageSize
        };

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);

        var totalRecords = await results.ReadSingleAsync<long>();
        var data = (await results.ReadAsync<TeamDto>()).ToList();

        return new PagedResult<TeamDto>(
            data,
            query.PageNumber,
            query.PageSize,
            totalRecords);
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

    private static string GetOrderBy(string sortBy, string sortDirection)
    {
        if (!SortColumns.TryGetValue(sortBy, out var column))
        {
            column = SortColumns["name"];
        }

        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        return $"{column} {direction}, t.\"Id\"";
    }
}
