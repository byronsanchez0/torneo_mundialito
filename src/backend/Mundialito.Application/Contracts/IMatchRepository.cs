using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface IMatchRepository
{
    Task<IReadOnlyCollection<Match>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
}

