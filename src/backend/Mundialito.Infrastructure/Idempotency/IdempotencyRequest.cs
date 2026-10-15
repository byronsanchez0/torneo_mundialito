namespace Mundialito.Infrastructure.Idempotency;

internal sealed class IdempotencyRequest
{
    private IdempotencyRequest() { }

    public IdempotencyRequest(
        string key,
        string requestHash,
        int responseStatusCode,
        string? responseBody,
        string? responseContentType)
    {
        Key = key;
        RequestHash = requestHash;
        ResponseStatusCode = responseStatusCode;
        ResponseBody = responseBody;
        ResponseContentType = responseContentType;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public int ResponseStatusCode { get; private set; }
    public string? ResponseBody { get; private set; }
    public string? ResponseContentType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
