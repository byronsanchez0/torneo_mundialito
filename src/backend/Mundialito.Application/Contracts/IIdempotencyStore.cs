namespace Mundialito.Application.Contracts;

public sealed record StoredIdempotencyResponse(
    string RequestHash,
    int StatusCode,
    string? ResponseBody,
    string? ContentType);

public interface IIdempotencyStore
{
    Task<StoredIdempotencyResponse?> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    void Add(
        string key,
        string requestHash,
        int statusCode,
        string? responseBody,
        string? contentType);
}
