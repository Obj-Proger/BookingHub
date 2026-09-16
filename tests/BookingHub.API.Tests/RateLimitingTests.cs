using System.Net;
using System.Net.Http.Json;
using BookingHub.API.Controllers.Auth;
using Testcontainers.PostgreSql;

namespace BookingHub.API.Tests;

/// <summary>
/// Deliberately not part of ApiCollection. This test intentionally exhausts the "public-write"
/// rate-limit bucket, which lives as one mutable, per-host singleton — TestServer's in-memory
/// transport doesn't give each test its own RemoteIpAddress, so every test sharing one host
/// shares one partition key. Running this against the shared collection host silently ate into
/// unrelated AuthenticationTests' quota (confirmed by an actual run where they started failing
/// with 429 instead of their expected status codes). A dedicated container+factory, paid for
/// once here, keeps that blast radius contained to this one class.
/// </summary>
public sealed class RateLimitingTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();
    private ApiWebApplicationFactory _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _factory = new ApiWebApplicationFactory(_container.GetConnectionString(), relaxRateLimits: false);
        await _factory.MigrateDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task PublicWriteEndpoint_ExceedsLimit_ReturnsTooManyRequests()
    {
        var client = _factory.CreateClient();

        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 11; i++)
        {
            lastResponse = await client.PostAsJsonAsync(
                "api/v1/auth/login", new LoginRequest($"{Guid.CreateVersion7()}@example.com", "Whatever123!"),
                TestContext.Current.CancellationToken);
        }

        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}