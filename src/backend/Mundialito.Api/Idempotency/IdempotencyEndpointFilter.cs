using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Mundialito.Application.Contracts;

namespace Mundialito.Api.Idempotency;

internal sealed class IdempotencyEndpointFilter : IEndpointFilter
{
    private const string HeaderName = "Idempotency-Key";
    private const int MaximumKeyLength = 200;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var cancellationToken = httpContext.RequestAborted;

        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var headerValue)
            || string.IsNullOrWhiteSpace(headerValue))
        {
            return Results.BadRequest(new
            {
                code = "Idempotency.KeyRequired",
                message = $"El header {HeaderName} es obligatorio.",
                type = "Validation"
            });
        }

        var key = headerValue.ToString().Trim();
        if (key.Length > MaximumKeyLength)
        {
            return Results.BadRequest(new
            {
                code = "Idempotency.KeyTooLong",
                message = $"El header {HeaderName} no puede superar {MaximumKeyLength} caracteres.",
                type = "Validation"
            });
        }

        var requestHash = await CalculateRequestHashAsync(httpContext.Request, cancellationToken);
        var store = httpContext.RequestServices.GetRequiredService<IIdempotencyStore>();
        var storedResponse = await store.GetAsync(key, cancellationToken);

        if (storedResponse is not null)
        {
            if (!string.Equals(storedResponse.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return Results.Conflict(new
                {
                    code = "Idempotency.KeyConflict",
                    message = "La Idempotency-Key ya fue utilizada con una solicitud diferente.",
                    type = "Conflict"
                });
            }

            return Replay(storedResponse);
        }

        var unitOfWork = httpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        object? endpointResult;
        try
        {
            endpointResult = await next(context);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var statusCode = GetStatusCode(endpointResult);
        if (statusCode is < 200 or >= 300)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return endpointResult;
        }

        var (responseBody, contentType) = SerializeResponse(endpointResult, httpContext);
        store.Add(key, requestHash, statusCode, responseBody, contentType);
        await unitOfWork.CompleteTransactionAsync(cancellationToken);

        return endpointResult;
    }

    private static async Task<string> CalculateRequestHashAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;

        var value = $"{request.Method}\n{request.Path}\n{body}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static int GetStatusCode(object? endpointResult) =>
        endpointResult is IStatusCodeHttpResult statusResult && statusResult.StatusCode.HasValue
            ? statusResult.StatusCode.Value
            : StatusCodes.Status200OK;

    private static (string? Body, string? ContentType) SerializeResponse(
        object? endpointResult,
        HttpContext httpContext)
    {
        if (endpointResult is not IValueHttpResult { Value: not null } valueResult)
        {
            return (null, null);
        }

        var options = httpContext.RequestServices
            .GetRequiredService<IOptions<JsonOptions>>()
            .Value.SerializerOptions;

        return (
            JsonSerializer.Serialize(valueResult.Value, valueResult.Value.GetType(), options),
            "application/json; charset=utf-8");
    }

    private static IResult Replay(StoredIdempotencyResponse response) =>
        response.ResponseBody is null
            ? Results.StatusCode(response.StatusCode)
            : Results.Content(
                response.ResponseBody,
                response.ContentType ?? "application/json; charset=utf-8",
                Encoding.UTF8,
                response.StatusCode);
}
