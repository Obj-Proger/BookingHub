using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class MeApiClient(AuthorizedApiClient authorizedClient) : IMeApiClient
{
    public Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetMyOrganizationsAsync(CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<MyOrganizationMembershipResponse>>("api/v1/me/organizations", cancellationToken);

    public Task<ApiResult<IReadOnlyList<EmployeeLocationResponse>>> GetMyEmployeeLocationsAsync(
        Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<EmployeeLocationResponse>>(
            $"api/v1/me/organizations/{organizationId}/locations", cancellationToken);
}