using System.Net.Http.Json;
using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

internal sealed class MeApiClient(HttpClient httpClient) : IMeApiClient
{
    public async Task<IReadOnlyList<MyOrganizationMembershipResponse>?> GetMyOrganizationsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("api/v1/me/organizations", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<MyOrganizationMembershipResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<IReadOnlyList<EmployeeLocationResponse>?> GetMyLocationsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/me/organizations/{organizationId}/locations", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeeLocationResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}