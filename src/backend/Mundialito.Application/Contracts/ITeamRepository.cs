using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface ITeamRepository
{
    Task<IReadOnlyCollection<Team>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
}

