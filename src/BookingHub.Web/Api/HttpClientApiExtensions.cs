using System.Net;

namespace BookingHub.Web.Api;

/// <summary>
/// Anonymous API calls with the same outcome shape as <see cref="Auth.AuthorizedApiClient"/>: every
/// answer becomes an <see cref="ApiResult"/> carrying the API's own explanation, and an unreachable
/// API becomes a 503 failure instead of an exception the page would have to catch.
/// </summary>
internal static class HttpClientApiExtensions
{
    private static readonly ApiError Unavailable = new(HttpStatusCode.ServiceUnavailable);

    public static async Task<ApiResult<T>> GetApiResultAsync<T>(
        this HttpClient httpClient, string uri, CancellationToken cancellationToken)
    {
        using var response = await TrySendAsync(() => httpClient.GetAsync(uri, cancellationToken), cancellationToken);
        return response is null
            ? ApiResult<T>.Failure(Unavailable)
            : await ApiResponseReader.ReadAsync<T>(response, cancellationToken);
    }

    public static async Task<ApiResult<T>> PostApiResultAsync<T>(
        this HttpClient httpClient, string uri, object body, CancellationToken cancellationToken)
    {
        using var response = await TrySendAsync(
            () => httpClient.PostAsJsonAsync(uri, body, ApiJsonOptions.Default, cancellationToken), cancellationToken);
        return response is null
            ? ApiResult<T>.Failure(Unavailable)
            : await ApiResponseReader.ReadAsync<T>(response, cancellationToken);
    }

    /// <summary>For endpoints that answer without a body.</summary>
    public static async Task<ApiResult> PostApiResultAsync(
        this HttpClient httpClient, string uri, object body, CancellationToken cancellationToken)
    {
        using var response = await TrySendAsync(
            () => httpClient.PostAsJsonAsync(uri, body, ApiJsonOptions.Default, cancellationToken), cancellationToken);
        return response is null
            ? ApiResult.Failure(Unavailable)
            : await ApiResponseReader.ReadAsync(response, cancellationToken);
    }

    /// <returns>Null when the API could not be reached.</returns>
    private static async Task<HttpResponseMessage?> TrySendAsync(
        Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null; // the HttpClient timeout, not the caller cancelling
        }
    }
}