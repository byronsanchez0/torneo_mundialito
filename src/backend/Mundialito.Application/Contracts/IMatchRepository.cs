using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AnyByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<bool> AnyGoalByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    void Add(Match match);
    void Remove(Match match);
}

