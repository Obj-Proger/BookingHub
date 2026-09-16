using BookingHub.API.Controllers.Locations;
using BookingHub.Application.Features.Locations.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace BookingHub.API.Tests;

[Collection(nameof(ApiCollection))]
public class TenantIsolationHttpTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task UserFromOrganizationA_CannotReadOrganizationBsLocation()
    {
        var client = fixture.Factory.CreateClient();

        var tokenA = await TestHelpers.RegisterAndLoginAsync(client, $"{Guid.CreateVersion7()}@example.com");
        var organizationAId = await TestHelpers.CreateOrganizationAsync(client, tokenA, "Org A", $"org-a-{Guid.CreateVersion7()}");

        var tokenB = await TestHelpers.RegisterAndLoginAsync(client, $"{Guid.CreateVersion7()}@example.com");
        var organizationBId = await TestHelpers.CreateOrganizationAsync(client, tokenB, "Org B", $"org-b-{Guid.CreateVersion7()}");

        using var createLocationRequest = new HttpRequestMessage(
            HttpMethod.Post, $"api/v1/organizations/{organizationBId}/locations")
        {
            Content = JsonContent.Create(new CreateLocationRequest(
                "Org B Branch", "221B Baker Street", "UTC",
                [.. Enum.GetValues<DayOfWeek>().Select(d => new DailyHoursDto(d, null, null))]))
        }.WithBearerToken(tokenB);
        await client.SendAsync(createLocationRequest, TestContext.Current.CancellationToken);

        using var crossTenantRequest = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/organizations/{organizationBId}/locations/{Guid.CreateVersion7()}")
            .WithBearerToken(tokenA);

        var response = await client.SendAsync(crossTenantRequest, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}