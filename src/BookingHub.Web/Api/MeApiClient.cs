using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class MeApiClient(AuthorizedApiClient authorizedClient) : IMeApiClient
{
    public async Task<IReadOnlyList<MyOrganizationMembershipResponse>?> GetMyOrganizationsAsync(CancellationToken cancellationToken)
    {
        using var response = await authorizedClient.SendAsync(
            () => new HttpRequestMessage(HttpMethod.Get, "api/v1/me/organizations"), cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<MyOrganizationMembershipResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}