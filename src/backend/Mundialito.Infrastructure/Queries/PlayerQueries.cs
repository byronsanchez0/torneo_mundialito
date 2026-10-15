using Dapper;
using Npgsql;
using Mundialito.Application.Common.Pagination;
using Mundialito.Application.Features.Players.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class PlayerQueries(NpgsqlDataSource dataSource) : IPlayerQueries
{
    public async Task<PagedResult<PlayerDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? teamId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM players p
            WHERE CAST(@TeamId AS uuid) IS NULL OR p."TeamId" = @TeamId;

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
            WHERE CAST(@TeamId AS uuid) IS NULL OR p."TeamId" = @TeamId
            ORDER BY p."LastName", p."FirstName", p."Id"
            LIMIT @PageSize OFFSET @Offset;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new
            {
                TeamId = teamId,
                PageSize = pageSize,
                Offset = (long)(page - 1) * pageSize
            },
            cancellationToken: cancellationToken);

        using var results = await connection.QueryMultipleAsync(command);
        var totalCount = await results.ReadSingleAsync<long>();
        var items = (await results.ReadAsync<PlayerDto>()).ToList();

        return new PagedResult<PlayerDto>(items, page, pageSize, totalCount);
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
}
