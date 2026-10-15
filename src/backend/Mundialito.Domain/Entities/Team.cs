namespace Mundialito.Domain.Entities;

public sealed class Team
{
    private Team() { }

    public Team(string name, string? coach = null)
    {
        Id = Guid.NewGuid();
        Name = Required(name, nameof(name));
        Coach = coach?.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Coach { get; private set; }

    public void Rename(string name) => Name = Required(name, nameof(name));

    public void ChangeCoach(string? coach) => Coach = coach?.Trim();

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El valor es obligatorio.", parameterName);
        }

        return value.Trim();
    }
}

