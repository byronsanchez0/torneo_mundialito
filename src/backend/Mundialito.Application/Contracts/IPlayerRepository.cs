using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ShirtNumberExistsAsync(
        Guid teamId,
        int shirtNumber,
        Guid? excludedPlayerId = null,
        CancellationToken cancellationToken = default);
    Task<bool> AnyByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    void Add(Player player);
    void Remove(Player player);
}

