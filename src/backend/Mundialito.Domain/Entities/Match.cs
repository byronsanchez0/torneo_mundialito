namespace Mundialito.Domain.Entities;

public sealed class Match
{
    private readonly List<Goal> _goals = [];

    private Match() { }

    public Match(Guid homeTeamId, Guid awayTeamId, DateTime scheduledAtUtc)
    {
        if (homeTeamId == Guid.Empty || awayTeamId == Guid.Empty)
        {
            throw new ArgumentException("Ambos equipos son obligatorios.");
        }

        if (homeTeamId == awayTeamId)
        {
            throw new ArgumentException("Un equipo no puede jugar contra sí mismo.");
        }

        Id = Guid.NewGuid();
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        ScheduledAtUtc = scheduledAtUtc.ToUniversalTime();
        Status = MatchStatus.Scheduled;
    }

    public Guid Id { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public MatchStatus Status { get; private set; }
    public IReadOnlyCollection<Goal> Goals => _goals;

    public void Start() => Status = MatchStatus.InProgress;

    public void Finish() => Status = MatchStatus.Finished;

    public Goal AddGoal(Guid playerId, Guid teamId, int minute)
    {
        if (Status == MatchStatus.Finished)
        {
            throw new InvalidOperationException("No se pueden registrar goles en un partido finalizado.");
        }

        if (teamId != HomeTeamId && teamId != AwayTeamId)
        {
            throw new ArgumentException("El equipo no participa en este partido.", nameof(teamId));
        }
        if (minute < 0 || minute > 120)
        {
            throw new ArgumentOutOfRangeException(nameof(minute), "El minuto del gol debe estar entre 0 y 120.");
        }

        var goal = new Goal(playerId, teamId, minute);
        _goals.Add(goal);
        return goal;
    }
}

