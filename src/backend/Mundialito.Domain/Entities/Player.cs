namespace Mundialito.Domain.Entities;

public sealed class Player
{
    private Player() { }

    public Player(Guid teamId, string firstName, string lastName, int shirtNumber, string position)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("El equipo es obligatorio.", nameof(teamId));
        }

        if (shirtNumber is < 1 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(shirtNumber), "El dorsal debe estar entre 1 y 99.");
        }

        Id = Guid.NewGuid();
        TeamId = teamId;
        FirstName = Required(firstName, nameof(firstName));
        LastName = Required(lastName, nameof(lastName));
        ShirtNumber = shirtNumber;
        Position = Required(position, nameof(position));
    }

    public Guid Id { get; private set; }
    public Guid TeamId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public int ShirtNumber { get; private set; }
    public string Position { get; private set; } = string.Empty;

    public void Update(
        Guid teamId,
        string firstName,
        string lastName,
        int shirtNumber,
        string position)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("El equipo es obligatorio.", nameof(teamId));
        }

        if (shirtNumber is < 1 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(shirtNumber), "El dorsal debe estar entre 1 y 99.");
        }

        TeamId = teamId;
        FirstName = Required(firstName, nameof(firstName));
        LastName = Required(lastName, nameof(lastName));
        ShirtNumber = shirtNumber;
        Position = Required(position, nameof(position));
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El valor es obligatorio.", parameterName);
        }

        return value.Trim();
    }
}

