using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class MatchRepository(MundialitoDbContext dbContext) : IMatchRepository
{
    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Matches
            .Include(match => match.Goals)
            .FirstOrDefaultAsync(match => match.Id == id, cancellationToken);

    public Task<bool> AnyByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        dbContext.Matches.AnyAsync(
            match => match.HomeTeamId == teamId || match.AwayTeamId == teamId,
            cancellationToken);

    public Task<bool> AnyGoalByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default) =>
        dbContext.Set<Goal>().AnyAsync(goal => goal.PlayerId == playerId, cancellationToken);

    public void Add(Match match) => dbContext.Matches.Add(match);

    public void Remove(Match match) => dbContext.Matches.Remove(match);
}

