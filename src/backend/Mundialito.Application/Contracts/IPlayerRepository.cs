using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface IPlayerRepository
{
    Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
}

