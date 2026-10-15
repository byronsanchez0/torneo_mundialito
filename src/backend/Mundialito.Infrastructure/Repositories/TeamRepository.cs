using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class TeamRepository(MundialitoDbContext dbContext) : ITeamRepository
{
    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Teams.FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludedTeamId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.Teams.AnyAsync(
            team => EF.Functions.ILike(team.Name, name)
                && (!excludedTeamId.HasValue || team.Id != excludedTeamId.Value),
            cancellationToken);

    public void Add(Team team) => dbContext.Teams.Add(team);

    public void Remove(Team team) => dbContext.Teams.Remove(team);
}

