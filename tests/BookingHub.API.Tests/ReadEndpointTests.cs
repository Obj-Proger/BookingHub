using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BookingHub.API.Controllers.Auth;
using BookingHub.Application.Features.Auth.DTOs;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.API.Tests;

/// <summary>
/// Calls every GET endpoint of the API, as the owner, against the demo data. Each one has to answer
/// successfully: a query EF Core cannot translate, or any other failure that only shows on a real
/// database, is caught here rather than by hand. New endpoints are picked up automatically; one with
/// a route value this test does not know fails until <see cref="RouteValuesAsync"/> supplies it.
/// </summary>
public partial class ReadEndpointTests(SeededApiFixture fixture) : IClassFixture<SeededApiFixture>
{
    [GeneratedRegex(@"\{(?<name>\w+)(:[^}]*)?\}")]
    private static partial Regex RouteParameter();

    [GeneratedRegex(@"/manage/(?<id>[0-9a-f-]{36})\?token=(?<token>\S+)")]
    private static partial Regex ManageLink();

    [Fact]
    public async Task EveryGetEndpoint_AsOwnerOnTheDemoData_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();

        var accessToken = await SignInAsOwnerAsync(client, cancellationToken);
        var (routeValues, guestToken) = await RouteValuesAsync(cancellationToken);
        var query = QueryString(guestToken);

        var routes = GetRoutes();
        routes.Should().HaveCountGreaterThan(10, "the endpoints are read from the running API");

        var failures = new List<string>();
        foreach (var route in routes)
        {
            var path = RouteParameter().Replace(route, match =>
                routeValues.TryGetValue(match.Groups["name"].Value, out var value)
                    ? value
                    : throw new InvalidOperationException($"No value for {{{match.Groups["name"].Value}}} in {route}: add one to the route values."));

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

    private static async Task<string> SignInAsOwnerAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(
            "api/v1/auth/login", new LoginRequest("owner@demo.test", DevelopmentSeeder.Password), cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(cancellationToken);
        return body!.AccessToken;
    }

    /// <summary>Ids of the seeded rows the endpoints are called with, and the guest token of Ivan's booking.</summary>
    private async Task<(Dictionary<string, string> RouteValues, string GuestToken)> RouteValuesAsync(CancellationToken cancellationToken)
    {
        var link = ManageLink().Match(fixture.SeedOutput);
        link.Success.Should().BeTrue("the seed prints a manage link for Ivan's booking");

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizationId = await dbContext.Organizations.IgnoreQueryFilters()
            .Select(o => o.Id).SingleAsync(cancellationToken);
        var locationId = await dbContext.Locations.IgnoreQueryFilters()
            .Where(l => l.Name == "Central").Select(l => l.Id).SingleAsync(cancellationToken);
        var employeeId = await dbContext.Employees.IgnoreQueryFilters()
            .Where(e => e.FullName == "Anna Smirnova").Select(e => e.Id).SingleAsync(cancellationToken);
        var assignmentId = await dbContext.EmployeeLocationAssignments.IgnoreQueryFilters()
            .Where(a => a.EmployeeId == employeeId && a.LocationId == locationId).Select(a => a.Id).SingleAsync(cancellationToken);
        var serviceId = await dbContext.Services.IgnoreQueryFilters()
            .Where(s => s.Name == "Haircut").Select(s => s.Id).SingleAsync(cancellationToken);
        var clientId = await dbContext.Clients.IgnoreQueryFilters()
            .Where(c => c.Name == "Ivan Petrov").Select(c => c.Id).SingleAsync(cancellationToken);

        var routeValues = new Dictionary<string, string>
        {
            ["organizationSlug"] = DevelopmentSeeder.OrganizationSlug,
            ["organizationId"] = Format(organizationId),
            ["locationId"] = Format(locationId),
            ["employeeId"] = Format(employeeId),
            ["assignmentId"] = Format(assignmentId),
            ["serviceId"] = Format(serviceId),
            ["clientId"] = Format(clientId),
            ["bookingId"] = link.Groups["id"].Value
        };

        return (routeValues, link.Groups["token"].Value);

        static string Format(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);
    }

    /// <summary>Every query parameter any GET endpoint takes; the ones an endpoint does not bind are ignored.</summary>
    private static string QueryString(string guestToken)
    {
        var utcNow = DateTime.UtcNow;
        var date = DateOnly.FromDateTime(utcNow.AddDays(2)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var fromUtc = utcNow.Date.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var toUtc = utcNow.Date.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        return $"?date={date}&fromUtc={fromUtc}&toUtc={toUtc}&phone=%2B79001000001&token={Uri.EscapeDataString(guestToken)}";
    }
}
