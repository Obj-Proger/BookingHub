using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class MeApiClient(AuthorizedApiClient authorizedClient) : IMeApiClient
{
    public Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetMyOrganizationsAsync(CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<MyOrganizationMembershipResponse>>("api/v1/me/organizations", cancellationToken);
}