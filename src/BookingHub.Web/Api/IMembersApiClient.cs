using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IMembersApiClient
{
    Task<ApiResult<IReadOnlyList<OrganizationMemberResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken);

    /// <summary>Adds a registered user; a 404 error means nobody has registered with this email.</summary>
    Task<ApiResult> AddByEmailAsync(
        Guid organizationId, string? email, OrganizationRole role, Guid? locationId, Guid? employeeId,
        CancellationToken cancellationToken);

    Task<ApiResult> ChangeRoleAsync(
        Guid organizationId, Guid memberId, OrganizationRole newRole, Guid? locationId, Guid? employeeId,
        CancellationToken cancellationToken);

    Task<ApiResult> RemoveAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken);
}