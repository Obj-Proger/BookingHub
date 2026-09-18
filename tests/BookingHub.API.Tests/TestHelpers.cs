using BookingHub.API.Controllers.Auth;
using BookingHub.API.Controllers.Organizations;
using BookingHub.Application.Features.Organizations.DTOs;
using System.Net.Http.Json;

namespace BookingHub.API.Tests;

internal static class TestHelpers
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password = "P@ssw0rd123")
    {
        await client.PostAsJsonAsync("api/v1/auth/register", new RegisterRequest(email, password));
        var loginResponse = await client.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password));
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedResponse>();
        return body!.AccessToken;
    }

    public static async Task<(string AccessToken, string RefreshToken)> RegisterAndLoginWithRefreshAsync(
        HttpClient client, string email, string password = "P@ssw0rd123")
    {
        await client.PostAsJsonAsync("api/v1/auth/register", new RegisterRequest(email, password));
        var loginResponse = await client.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password));
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedResponse>();
        return (body!.AccessToken, body.RefreshToken);
    }

    public static async Task<Guid> CreateOrganizationAsync(HttpClient client, string token, string name, string slug)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/organizations")
        {
            Content = JsonContent.Create(new CreateOrganizationRequest(name, slug))
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<OrganizationCreatedResponse>();
        return body!.OrganizationId;
    }

    public static HttpRequestMessage WithBearerToken(this HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}