using System.Globalization;
using System.Text.RegularExpressions;
using BookingHub.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.API.Tests;

/// <summary>
/// Calls every GET endpoint of the API against the demo data — the caller's own (<c>me</c>) endpoints as
/// the employee Anna, every other one as the owner. Each one has to answer successfully: a query EF Core
/// cannot translate, or any other failure that only shows on a real database, is caught here rather
/// than by hand. New endpoints are picked up automatically; one with a route value this test does not
/// know fails until <see cref="RouteValues"/> supplies it.
/// </summary>
public partial class ReadEndpointTests(SeededApiFixture fixture) : IClassFixture<SeededApiFixture>
{
    [GeneratedRegex(@"\{(?<name>\w+)(:[^}]*)?\}")]
    private static partial Regex RouteParameter();

    [Fact]
    public async Task EveryGetEndpoint_OnTheDemoData_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();

        var ownerToken = await TestHelpers.SignInAsync(client, DemoData.OwnerEmail, DevelopmentSeeder.Password, cancellationToken);
        var employeeToken = await TestHelpers.SignInAsync(client, DemoData.EmployeeEmail, DevelopmentSeeder.Password, cancellationToken);
        var routeValues = RouteValues(fixture.Demo);
        var query = QueryString(fixture.Demo.IvanBooking.Token);

        var routes = GetRoutes();
        routes.Should().HaveCountGreaterThan(10, "the endpoints are read from the running API");

        var failures = new List<string>();
        foreach (var route in routes)
        {
            var path = RouteParameter().Replace(route, match =>
                routeValues.TryGetValue(match.Groups["name"].Value, out var value)
                    ? value
                    : throw new InvalidOperationException($"No value for {{{match.Groups["name"].Value}}} in {route}: add one to the route values."));

            // The "me" endpoints answer for the caller's own employee record, which the owner does not have.
            var accessToken = route.StartsWith("api/v1/me/", StringComparison.Ordinal) ? employeeToken : ownerToken;

            using var request = new HttpRequestMessage(HttpMethod.Get, path + query).WithBearerToken(accessToken);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                failures.Add($"GET {route} -> {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(cancellationToken)}");
        }

        failures.Should().BeEmpty();
    }

    private List<string> GetRoutes() =>
    [
        .. fixture.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Get) == true)
            .Select(endpoint => (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('~', '/'))
            .Where(route => route.StartsWith("api/", StringComparison.Ordinal))
            .Distinct()
            .Order(StringComparer.Ordinal)
    ];

    /// <summary>The seeded rows the endpoints are called with: Anna's haircuts at Central and Ivan's booking.</summary>
    private static Dictionary<string, string> RouteValues(DemoData demo) => new()
    {
        ["organizationSlug"] = DevelopmentSeeder.OrganizationSlug,
        ["organizationId"] = Format(demo.OrganizationId),
        ["locationId"] = Format(demo.CentralId),
        ["employeeId"] = Format(demo.AnnaId),
        ["assignmentId"] = Format(demo.AnnaAtCentralId),
        ["serviceId"] = Format(demo.HaircutId),
        ["clientId"] = Format(demo.IvanId),
        ["bookingId"] = Format(demo.IvanBooking.Id)
    };

    private static string Format(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);

    /// <summary>Every query parameter any GET endpoint takes; the ones an endpoint does not bind are ignored.</summary>
    private static string QueryString(string guestToken)
    {
        var utcNow = DateTime.UtcNow;
        var date = DateOnly.FromDateTime(utcNow.AddDays(2)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var fromUtc = utcNow.Date.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var toUtc = utcNow.Date.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var phone = Uri.EscapeDataString(DemoData.IvanPhone);

        return $"?date={date}&fromUtc={fromUtc}&toUtc={toUtc}&phone={phone}&token={Uri.EscapeDataString(guestToken)}";
    }
}
