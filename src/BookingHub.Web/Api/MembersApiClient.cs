using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class MembersApiClient(AuthorizedApiClient authorizedClient) : IMembersApiClient
{
    private static string BasePath(Guid organizationId) => $"api/v1/organizations/{organizationId}/members";

    public Task<ApiResult<IReadOnlyList<OrganizationMemberResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<OrganizationMemberResponse>>(BasePath(organizationId), cancellationToken);

    public Task<ApiResult> AddByEmailAsync(
        Guid organizationId, string? email, OrganizationRole role, Guid? locationId, Guid? employeeId,
        CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Post, $"{BasePath(organizationId)}/by-email",
            new AddOrganizationMemberByEmailRequest(email, role, locationId, employeeId), cancellationToken);

    public Task<ApiResult> ChangeRoleAsync(
        Guid organizationId, Guid memberId, OrganizationRole newRole, Guid? locationId, Guid? employeeId,
        CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Patch, $"{BasePath(organizationId)}/{memberId}",
            new ChangeOrganizationMemberRoleRequest(newRole, locationId, employeeId), cancellationToken);

    public Task<ApiResult> RemoveAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(HttpMethod.Delete, $"{BasePath(organizationId)}/{memberId}", body: null, cancellationToken);
}