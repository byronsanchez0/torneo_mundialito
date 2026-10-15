using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Infrastructure.Persistence;

namespace Mundialito.Infrastructure.Idempotency;

internal sealed class IdempotencyStore(MundialitoDbContext dbContext) : IIdempotencyStore
{
    public async Task<StoredIdempotencyResponse?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var request = await dbContext.Set<IdempotencyRequest>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Key == key, cancellationToken);

        return request is null
            ? null
            : new StoredIdempotencyResponse(
                request.RequestHash,
                request.ResponseStatusCode,
                request.ResponseBody,
                request.ResponseContentType);
    }

    public void Add(
        string key,
        string requestHash,
        int statusCode,
        string? responseBody,
        string? contentType)
    {
        dbContext.Set<IdempotencyRequest>().Add(new IdempotencyRequest(
            key,
            requestHash,
            statusCode,
            responseBody,
            contentType));
    }
}
