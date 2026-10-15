using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Players.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class PlayerQueries(NpgsqlDataSource dataSource) : IPlayerQueries
{
    private static readonly IReadOnlyDictionary<string, string> SortColumns =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["firstName"] = "p.\"FirstName\"",
            ["lastName"] = "p.\"LastName\"",
            ["name"] = "p.\"LastName\"",
            ["teamName"] = "t.\"Name\"",
            ["shirtNumber"] = "p.\"ShirtNumber\"",
            ["position"] = "p.\"Position\""
        };

    private const string Filters = """
        WHERE (CAST(@TeamId AS uuid) IS NULL OR p."TeamId" = @TeamId)
          AND (
              CAST(@Name AS text) IS NULL
              OR CONCAT(p."FirstName", ' ', p."LastName") ILIKE '%' || @Name || '%'
          )
          AND (
              CAST(@Position AS text) IS NULL
              OR p."Position" ILIKE '%' || @Position || '%'
          )
          AND (CAST(@ShirtNumber AS integer) IS NULL OR p."ShirtNumber" = @ShirtNumber)
        """;

    public async Task<PagedResult<PlayerDto>> GetPagedAsync(
        GetPlayersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orderBy = GetOrderBy(query.SortBy, query.SortDirection);
        var sql = $$"""
            SELECT COUNT(*)
            FROM players p
            {{Filters}};

            SELECT
                p."Id" AS "Id",
                p."TeamId" AS "TeamId",
                t."Name" AS "TeamName",
                p."FirstName" AS "FirstName",
                p."LastName" AS "LastName",
                p."ShirtNumber" AS "ShirtNumber",
                p."Position" AS "Position"
            FROM players p
            INNER JOIN teams t ON t."Id" = p."TeamId"
            {{Filters}}
            ORDER BY {{orderBy}}
            LIMIT @PageSize OFFSET @Offset;
            """;

        var parameters = new
        {
            query.TeamId,
            query.Name,
            query.Position,
            query.ShirtNumber,
            query.PageSize,
            Offset = (long)(query.PageNumber - 1) * query.PageSize
        };

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);

        var totalRecords = await results.ReadSingleAsync<long>();
        var data = (await results.ReadAsync<PlayerDto>()).ToList();

        return new PagedResult<PlayerDto>(
            data,
            query.PageNumber,
            query.PageSize,
            totalRecords);
    }

    public async Task<PlayerDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p."Id" AS "Id",
                p."TeamId" AS "TeamId",
                t."Name" AS "TeamName",
                p."FirstName" AS "FirstName",
                p."LastName" AS "LastName",
                p."ShirtNumber" AS "ShirtNumber",
                p."Position" AS "Position"
            FROM players p
            INNER JOIN teams t ON t."Id" = p."TeamId"
            WHERE p."Id" = @Id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PlayerDto>(command);
    }

    private static string GetOrderBy(string sortBy, string sortDirection)
    {
        if (!SortColumns.TryGetValue(sortBy, out var column))
        {
            column = SortColumns["lastName"];
        }

        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        return $"{column} {direction}, p.\"Id\"";
    }
}
