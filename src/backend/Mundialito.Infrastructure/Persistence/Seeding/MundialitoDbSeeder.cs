using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence.Seeding;

internal sealed class MundialitoDbSeeder(
    MundialitoDbContext dbContext,
    IUnitOfWork unitOfWork)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Teams.AsNoTracking().AnyAsync(cancellationToken))
        {
            return;
        }

        var teams = CreateTeams();
        var players = CreatePlayers(teams);
        var matches = CreateMatches(teams, players);

        dbContext.Teams.AddRange(teams);
        dbContext.Players.AddRange(players);
        dbContext.Matches.AddRange(matches);

        await unitOfWork.CommitAsync(cancellationToken);
    }

    private static Team[] CreateTeams() =>
    [
        new("Liverpool", "Carlos Méndez"),
        new("Barcelona", "Andrea López"),
        new("Real Madrid", "Miguel Herrera"),
        new("PSG", "Sofía Castillo")
    ];

    private static Player[] CreatePlayers(IReadOnlyList<Team> teams) =>
    [
        new(teams[0].Id, "Miguel Alberto", "Rodriguez Barrientos", 1, "Portero"),
        new(teams[0].Id, "Cesar Mauricio", "Guerrero Batres", 4, "Defensa"),
        new(teams[0].Id, "Juan Carlos", "Juarez Ruano", 6, "Mediocampista"),
        new(teams[0].Id, "Pablo", "Díaz", 9, "Delantero"),
        new(teams[0].Id, "José", "Ramírez", 10, "Delantero"),

        new(teams[1].Id, "Mario", "Gómez", 1, "Portero"),
        new(teams[1].Id, "Edwin", "Pérez", 4, "Defensa"),
        new(teams[1].Id, "Kevin", "Flores", 6, "Mediocampista"),
        new(teams[1].Id, "Óscar", "Cruz", 9, "Delantero"),
        new(teams[1].Id, "Javier", "Ortiz", 10, "Delantero"),

        new(teams[2].Id, "Daniel", "Ruiz", 1, "Portero"),
        new(teams[2].Id, "Hugo", "Santos", 4, "Defensa"),
        new(teams[2].Id, "Ángel", "Vargas", 6, "Mediocampista"),
        new(teams[2].Id, "Raúl", "Molina", 9, "Delantero"),
        new(teams[2].Id, "César", "Aguilar", 10, "Delantero"),

        new(teams[3].Id, "David", "Castro", 1, "Portero"),
        new(teams[3].Id, "Erick", "Lemus", 4, "Defensa"),
        new(teams[3].Id, "Bryan", "Vega", 6, "Mediocampista"),
        new(teams[3].Id, "Andrés", "Cabrera", 9, "Delantero"),
        new(teams[3].Id, "Fernando", "Rojas", 10, "Delantero")
    ];

    private static Match[] CreateMatches(
        IReadOnlyList<Team> teams,
        IReadOnlyList<Player> players)
    {
        var today = DateTime.UtcNow.Date;
        var matches = new[]
        {
            new Match(teams[0].Id, teams[1].Id, today.AddDays(-3).AddHours(18)),
            new Match(teams[2].Id, teams[3].Id, today.AddDays(-2).AddHours(18)),
            new Match(teams[0].Id, teams[2].Id, today.AddDays(-1).AddHours(18)),
            new Match(teams[1].Id, teams[3].Id, today.AddDays(1).AddHours(18)),
            new Match(teams[0].Id, teams[3].Id, today.AddDays(2).AddHours(18)),
            new Match(teams[1].Id, teams[2].Id, today.AddDays(3).AddHours(18))
        };

        FinishMatch(matches[0],
            (players[3], teams[0], 14),
            (players[8], teams[1], 36),
            (players[4], teams[0], 71));

        FinishMatch(matches[1],
            (players[13], teams[2], 28),
            (players[18], teams[3], 64));

        FinishMatch(matches[2],
            (players[4], teams[0], 22),
            (players[13], teams[2], 51),
            (players[14], teams[2], 83));

        return matches;
    }

    private static void FinishMatch(
        Match match,
        params (Player Player, Team Team, int Minute)[] goals)
    {
        match.Start();

        foreach (var goal in goals)
        {
            match.AddGoal(goal.Player.Id, goal.Team.Id, goal.Minute);
        }

        match.Finish();
    }
}
