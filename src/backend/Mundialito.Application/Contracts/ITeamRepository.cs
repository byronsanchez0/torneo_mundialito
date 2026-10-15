using Mundialito.Domain.Entities;

namespace Mundialito.Application.Contracts;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(
        string name,
        Guid? excludedTeamId = null,
        CancellationToken cancellationToken = default);
    void Add(Team team);
    void Remove(Team team);
}

