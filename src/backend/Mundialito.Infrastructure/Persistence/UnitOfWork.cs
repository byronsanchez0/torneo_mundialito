using Microsoft.EntityFrameworkCore.Storage;
using Mundialito.Application.Contracts;

namespace Mundialito.Infrastructure.Persistence;

internal sealed class UnitOfWork(MundialitoDbContext dbContext) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return affectedRows;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
