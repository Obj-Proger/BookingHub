using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookingHub.Web.Api;

internal static class ApiResponseReader
{
    public static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadProblemMessageAsync(response, cancellationToken);
            return ApiResult<T>.Failure(new ApiError(response.StatusCode, message));
        }

        var value = await response.Content.ReadFromJsonAsync<T>(ApiJsonOptions.Default, cancellationToken);
        return value is null
            ? ApiResult<T>.Failure(new ApiError(response.StatusCode, "The server returned an empty response."))
            : ApiResult<T>.Success(value);
    }

    private static async Task<string> ReadProblemMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(ApiJsonOptions.Default, cancellationToken);

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return problem.Detail;

            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return problem.Title;
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }

        return DefaultMessage(response.StatusCode);
    }

    private static string DefaultMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => "Your session has expired.",
        HttpStatusCode.Forbidden => "You don't have access to this.",
        HttpStatusCode.NotFound => "Not found.",
        HttpStatusCode.TooManyRequests => "Too many requests. Please wait a moment and try again.",
        HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again.",
        _ => "Something went wrong. Please try again."
    };

    private sealed record ProblemResponse(string? Title, string? Detail);
}