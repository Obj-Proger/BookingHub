using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicCatalogApiClient(HttpClient httpClient) : IPublicCatalogApiClient
{
    public async Task<IReadOnlyList<PublicLocationResponse>?> GetLocationsAsync(string organizationSlug, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/public/{organizationSlug}/locations", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<PublicLocationResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    /// <summary>Resolved from the public location list — the API has no single-location endpoint.</summary>
    public async Task<PublicLocationResponse?> GetLocationAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken)
    {
        var locations = await GetLocationsAsync(organizationSlug, cancellationToken);
        return locations?.FirstOrDefault(location => location.LocationId == locationId);
    }

    public async Task<IReadOnlyList<PublicServiceResponse>?> GetServicesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/public/{organizationSlug}/locations/{locationId}/services", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<PublicServiceResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<IReadOnlyList<PublicEmployeeResponse>?> GetEmployeesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/public/{organizationSlug}/locations/{locationId}/employees", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<PublicEmployeeResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}