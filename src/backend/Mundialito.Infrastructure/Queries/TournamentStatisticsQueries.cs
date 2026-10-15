using Dapper;
using Npgsql;
using Mundialito.Application.Features.Statistics.Queries;

namespace Mundialito.Infrastructure.Queries;

internal sealed class TournamentStatisticsQueries(NpgsqlDataSource dataSource)
    : ITournamentStatisticsQueries
{
    public async Task<TournamentStatisticsDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH match_scores AS (
                SELECT
                    m."HomeTeamId",
                    m."AwayTeamId",
                    COUNT(g."Id") FILTER (WHERE g."TeamId" = m."HomeTeamId")::int AS home_score,
                    COUNT(g."Id") FILTER (WHERE g."TeamId" = m."AwayTeamId")::int AS away_score
                FROM matches m
                LEFT JOIN goals g ON g."MatchId" = m."Id"
                WHERE m."Status" = 'Finished'
                GROUP BY m."Id", m."HomeTeamId", m."AwayTeamId"
            ),
            team_results AS (
                SELECT
                    "HomeTeamId" AS team_id,
                    home_score AS goals_for,
                    away_score AS goals_against
                FROM match_scores

                UNION ALL

                SELECT
                    "AwayTeamId" AS team_id,
                    away_score AS goals_for,
                    home_score AS goals_against
                FROM match_scores
            )
            SELECT
                t."Id" AS "TeamId",
                t."Name" AS "TeamName",
                COUNT(r.team_id)::int AS "Played",
                COUNT(*) FILTER (WHERE r.goals_for > r.goals_against)::int AS "Won",
                COUNT(*) FILTER (WHERE r.goals_for = r.goals_against)::int AS "Drawn",
                COUNT(*) FILTER (WHERE r.goals_for < r.goals_against)::int AS "Lost",
                COALESCE(SUM(r.goals_for), 0)::int AS "GoalsFor",
                COALESCE(SUM(r.goals_against), 0)::int AS "GoalsAgainst",
                COALESCE(SUM(r.goals_for - r.goals_against), 0)::int AS "GoalDifference",
                COALESCE(SUM(
                    CASE
                        WHEN r.goals_for > r.goals_against THEN 3
                        WHEN r.goals_for = r.goals_against THEN 1
                        ELSE 0
                    END
                ), 0)::int AS "Points"
            FROM teams t
            LEFT JOIN team_results r ON r.team_id = t."Id"
            GROUP BY t."Id", t."Name"
            ORDER BY "Points" DESC, "GoalDifference" DESC, "GoalsFor" DESC, t."Name";

            SELECT
                p."Id" AS "PlayerId",
                CONCAT(p."FirstName", ' ', p."LastName") AS "PlayerName",
                t."Name" AS "TeamName",
                COUNT(g."Id")::int AS "Goals"
            FROM goals g
            INNER JOIN players p ON p."Id" = g."PlayerId"
            INNER JOIN teams t ON t."Id" = p."TeamId"
            GROUP BY p."Id", p."FirstName", p."LastName", t."Name"
            ORDER BY "Goals" DESC, "PlayerName", p."Id"
            LIMIT 10;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);

        var standings = (await results.ReadAsync<StandingDto>()).ToList();
        var topScorers = (await results.ReadAsync<TopScorerDto>()).ToList();

        return new TournamentStatisticsDto(standings, topScorers);
    }
}
