namespace Mundialito.Domain.Entities;

public sealed class Goal
{
    private Goal() { }

    internal Goal(Guid playerId, Guid teamId, int minute)
    {
        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("El jugador es obligatorio.", nameof(playerId));
        }

        if (minute is < 1 or > 130)
        {
            throw new ArgumentOutOfRangeException(nameof(minute), "El minuto debe estar entre 1 y 130.");
        }

        Id = Guid.NewGuid();
        PlayerId = playerId;
        TeamId = teamId;
        Minute = minute;
    }

    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid TeamId { get; private set; }
    public int Minute { get; private set; }
}

