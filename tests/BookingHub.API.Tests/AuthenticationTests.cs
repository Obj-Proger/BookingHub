using AwesomeAssertions;
using BookingHub.API.Controllers.Auth;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace BookingHub.API.Tests;

[Collection(nameof(ApiCollection))]
public class AuthenticationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task Register_ThenLogin_ReturnsWorkingToken()
    {
        var client = fixture.Factory.CreateClient();
        var email = $"{Guid.CreateVersion7()}@example.com";

        var token = await TestHelpers.RegisterAndLoginAsync(client, email);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = fixture.Factory.CreateClient();
        var email = $"{Guid.CreateVersion7()}@example.com";
        await client.PostAsJsonAsync(
            "api/v1/auth/register", new RegisterRequest(email, "P@ssw0rd123"), TestContext.Current.CancellationToken);

        var response = await client.PostAsJsonAsync(
            "api/v1/auth/login", new LoginRequest(email, "WrongPassword1!"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonexistentEmail_ReturnsUnauthorizedNotServerError()
    {
        var client = fixture.Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/v1/auth/login", new LoginRequest($"{Guid.CreateVersion7()}@example.com", "Whatever123!"),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_NoToken_ReturnsUnauthorized()
    {
        var client = fixture.Factory.CreateClient();

        var response = await client.GetAsync($"api/v1/organizations/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_ValidToken_IsNotRejectedByAuthentication()
    {
        var client = fixture.Factory.CreateClient();
        var token = await TestHelpers.RegisterAndLoginAsync(client, $"{Guid.CreateVersion7()}@example.com");
        var organizationId = await TestHelpers.CreateOrganizationAsync(client, token, "Org", $"org-{Guid.CreateVersion7()}");

        // The caller is a genuine member (Owner) of organizationId — IRequireOrganizationMembership
        // passes — so a 404 here can only come from GetLocationQueryHandler itself ("no such
        // location"), proving the token was accepted and the request reached business logic,
        // not rejected earlier by authentication or authorization. GetOrganizationQuery itself
        // can't demonstrate this: it requires membership too, so any org the caller doesn't
        // belong to always answers 403 regardless of whether that org exists.
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/organizations/{organizationId}/locations/{Guid.CreateVersion7()}")
            .WithBearerToken(token);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewWorkingTokens()
    {
        var client = fixture.Factory.CreateClient();
        var (accessToken, refreshToken) = await TestHelpers.RegisterAndLoginWithRefreshAsync(client, $"{Guid.CreateVersion7()}@example.com");

        var response = await client.PostAsJsonAsync("api/v1/auth/refresh", new RefreshRequest(refreshToken), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().NotBe(accessToken);
        body.RefreshToken.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task Refresh_ReusedRotatedToken_FailsAndRevokesTheReplacementToo()
    {
        var client = fixture.Factory.CreateClient();
        var (_, refreshToken) = await TestHelpers.RegisterAndLoginWithRefreshAsync(client, $"{Guid.CreateVersion7()}@example.com");

        var firstRefresh = await client.PostAsJsonAsync("api/v1/auth/refresh", new RefreshRequest(refreshToken), TestContext.Current.CancellationToken);
        var firstBody = await firstRefresh.Content.ReadFromJsonAsync<AuthenticatedResponse>(TestContext.Current.CancellationToken);

        // Reusing the original (now-rotated) token — this is the theft-detection path.
        var reuseAttempt = await client.PostAsJsonAsync("api/v1/auth/refresh", new RefreshRequest(refreshToken), TestContext.Current.CancellationToken);

        // The legitimate replacement token from the first refresh should now be revoked too.
        var secondAttempt = await client.PostAsJsonAsync(
            "api/v1/auth/refresh", new RefreshRequest(firstBody!.RefreshToken), TestContext.Current.CancellationToken);

        reuseAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        secondAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}