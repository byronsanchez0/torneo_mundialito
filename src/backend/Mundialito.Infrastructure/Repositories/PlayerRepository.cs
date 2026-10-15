using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class PlayerRepository(MundialitoDbContext dbContext) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Players.FirstOrDefaultAsync(player => player.Id == id, cancellationToken);

    public Task<bool> ShirtNumberExistsAsync(
        Guid teamId,
        int shirtNumber,
        Guid? excludedPlayerId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.Players.AnyAsync(
            player => player.TeamId == teamId
                && player.ShirtNumber == shirtNumber
                && (!excludedPlayerId.HasValue || player.Id != excludedPlayerId.Value),
            cancellationToken);

    public Task<bool> AnyByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        dbContext.Players.AnyAsync(player => player.TeamId == teamId, cancellationToken);

    public void Add(Player player) => dbContext.Players.Add(player);

    public void Remove(Player player) => dbContext.Players.Remove(player);
}

