using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class ClientsApiClient(AuthorizedApiClient authorizedClient) : IClientsApiClient
{
    private static string BasePath(Guid organizationId) => $"api/v1/organizations/{organizationId}/clients";

    public Task<ApiResult<ClientSearchResultResponse>> SearchByPhoneAsync(
        Guid organizationId, string phone, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<ClientSearchResultResponse>(
            $"{BasePath(organizationId)}/search?phone={Uri.EscapeDataString(phone)}", cancellationToken);

    public Task<ApiResult<ClientProfileResponse>> GetProfileAsync(
        Guid organizationId, Guid clientId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<ClientProfileResponse>($"{BasePath(organizationId)}/{clientId}", cancellationToken);
}