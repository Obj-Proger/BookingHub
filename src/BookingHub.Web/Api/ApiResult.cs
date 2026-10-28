using System.Net;

namespace BookingHub.Web.Api;

/// <param name="ServerMessage">The explanation the API gave, if any; without one the UI picks a text for the status code.</param>
public sealed record ApiError(HttpStatusCode StatusCode, string? ServerMessage = null)
{
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}

public sealed class ApiResult
{
    private ApiResult(ApiError? error) => Error = error;

    public bool IsSuccess => Error is null;

    public ApiError? Error { get; }

    public static ApiResult Success() => new(null);

    public static ApiResult Failure(ApiError error) => new(error);
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

    /// <summary>The same outcome without the value, for callers that only need to know whether the call succeeded.</summary>
    public ApiResult WithoutValue() => IsSuccess ? ApiResult.Success() : ApiResult.Failure(Error!);
}