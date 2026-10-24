using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class OrganizationsApiClient(AuthorizedApiClient authorizedClient) : IOrganizationsApiClient
{
    public async Task<ApiResult<OrganizationCreatedResponse>> CreateAsync(string? name, string? slug, CancellationToken cancellationToken)
    {
        using var response = await authorizedClient.SendAsync(
            () => new HttpRequestMessage(HttpMethod.Post, "api/v1/organizations")
            {
                Content = JsonContent.Create(new CreateOrganizationRequest(name, slug), options: ApiJsonOptions.Default)
            },
            cancellationToken);

        return await ApiResponseReader.ReadAsync<OrganizationCreatedResponse>(response, cancellationToken);
    }
}