namespace Mundialito.Application.Features.Statistics.Queries;

public sealed record StandingDto(
    Guid TeamId,
    string TeamName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);

public sealed record TopScorerDto(
    Guid PlayerId,
    string PlayerName,
    string TeamName,
    int Goals);

public sealed record TournamentStatisticsDto(
    IReadOnlyCollection<StandingDto> Standings,
    IReadOnlyCollection<TopScorerDto> TopScorers);

public interface ITournamentStatisticsQueries
{
    Task<TournamentStatisticsDto> GetAsync(
        CancellationToken cancellationToken = default);
}

public sealed class GetTournamentStatisticsQueryHandler(
    ITournamentStatisticsQueries statisticsQueries)
{
    public Task<TournamentStatisticsDto> HandleAsync(
        CancellationToken cancellationToken = default) =>
        statisticsQueries.GetAsync(cancellationToken);
}
