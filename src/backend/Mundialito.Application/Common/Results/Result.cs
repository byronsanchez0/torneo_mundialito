namespace Mundialito.Application.Common.Results;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public string? ErrorMessage => Error?.Message;

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);
}
