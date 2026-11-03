using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingHub.API.Tests;

/// <summary>
/// Calls the API as one caller — a guest, or a signed-in account when a token is given — with the JSON
/// options the API speaks. A call the API refuses fails the test with the response body, so a broken
/// step says why; <see cref="SendAsync"/> is there for the calls that are meant to be refused.
/// </summary>
public sealed class TestApi(HttpClient client, string? accessToken = null)
{
    /// <summary>Web defaults, and enums as names, as the API writes them.</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Get, url, null, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken))!;
    }

    public async Task<T> PostAsync<T>(string url, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Post, url, body, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken))!;
    }

    public async Task PostAsync(string url, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Post, url, body, cancellationToken);
    }

    public async Task PutAsync(string url, object body, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Put, url, body, cancellationToken);
    }

    public async Task PatchAsync(string url, object body, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Patch, url, body, cancellationToken);
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await SendSuccessfullyAsync(HttpMethod.Delete, url, null, cancellationToken);
    }

    /// <summary>Sends the request and returns the response whatever its status; the caller disposes it.</summary>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        if (accessToken is not null)
            request.WithBearerToken(accessToken);

        return await client.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendSuccessfullyAsync(
        HttpMethod method, string url, object? body, CancellationToken cancellationToken)
    {
        var response = await SendAsync(method, url, body, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            response.Dispose();
            Assert.Fail($"{method} {url} answered {(int)response.StatusCode}: {content}");
        }

        return response;
    }
}
