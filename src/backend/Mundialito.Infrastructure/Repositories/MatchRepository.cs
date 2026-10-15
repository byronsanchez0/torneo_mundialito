using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Domain.Entities;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Repositories;

internal sealed class MatchRepository(MundialitoDbContext dbContext) : IMatchRepository
{
    public async Task<IReadOnlyCollection<Match>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Matches.AsNoTracking()
            .Include(match => match.Goals)
            .OrderBy(match => match.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Matches.AsNoTracking()
            .Include(match => match.Goals)
            .FirstOrDefaultAsync(match => match.Id == id, cancellationToken);

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        dbContext.Matches.Add(match);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

