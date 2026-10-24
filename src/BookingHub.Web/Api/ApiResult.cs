using System.Net;

namespace BookingHub.Web.Api;

public sealed record ApiError(HttpStatusCode StatusCode, string Message)
{
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}

public sealed class ApiResult<T>
{
    private readonly T? _value;

    private ApiResult(T? value, ApiError? error)
    {
        _value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public ApiError? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The result is a failure and has no value.");

    public static ApiResult<T> Success(T value) => new(value, null);

    public static ApiResult<T> Failure(ApiError error) => new(default, error);
}