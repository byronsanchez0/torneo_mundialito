using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class PlayerRepository(MundialitoDbContext dbContext) : IPlayerRepository
{
    public async Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Players.AsNoTracking()
            .OrderBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Players.AsNoTracking().FirstOrDefaultAsync(player => player.Id == id, cancellationToken);

    public async Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

