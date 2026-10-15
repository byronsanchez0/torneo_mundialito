using Mundialito.Application.Common.Results;
using ApplicationResult = Mundialito.Application.Common.Results.Result;

namespace Mundialito.Api.Endpoints;

internal static class ResultHttpExtensions
{
    public static IResult ToHttpFailure(this ApplicationResult result)
    {
        if (result.Error is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error inesperado.");
        }

        var body = new ErrorResponse(
            result.Error.Code,
            result.Error.Message,
            result.Error.Type.ToString());

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(body),
            ErrorType.NotFound => Results.NotFound(body),
            ErrorType.Conflict => Results.Conflict(body),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: result.Error.Message,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = result.Error.Code
                })
        };
    }

    private sealed record ErrorResponse(string Code, string Message, string Type);
}
