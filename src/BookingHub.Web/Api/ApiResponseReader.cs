using System.Net.Http.Json;
using System.Text.Json;

namespace BookingHub.Web.Api;

internal static class ApiResponseReader
{
    public static async Task<ApiResult> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return ApiResult.Success();

        var message = await ReadProblemMessageAsync(response, cancellationToken);
        return ApiResult.Failure(new ApiError(response.StatusCode, message));
    }

    public static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadProblemMessageAsync(response, cancellationToken);
            return ApiResult<T>.Failure(new ApiError(response.StatusCode, message));
        }

        var value = await response.Content.ReadFromJsonAsync<T>(ApiJsonOptions.Default, cancellationToken);
        return value is null
            ? ApiResult<T>.Failure(new ApiError(response.StatusCode))
            : ApiResult<T>.Success(value);
    }

    /// <returns>The API's explanation, or null when the body has none.</returns>
    private static async Task<string?> ReadProblemMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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

        return null;
    }

    private sealed record ProblemResponse(string? Title, string? Detail);
}