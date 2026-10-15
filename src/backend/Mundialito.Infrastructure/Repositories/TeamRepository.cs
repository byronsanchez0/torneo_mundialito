using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class TeamRepository(MundialitoDbContext dbContext) : ITeamRepository
{
    public async Task<IReadOnlyCollection<Team>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Teams.AsNoTracking().OrderBy(team => team.Name).ToListAsync(cancellationToken);

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Teams.AsNoTracking().FirstOrDefaultAsync(team => team.Id == id, cancellationToken);

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

