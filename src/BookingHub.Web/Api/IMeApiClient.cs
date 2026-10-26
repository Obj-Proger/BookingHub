using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IMeApiClient
{
    Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetMyOrganizationsAsync(CancellationToken cancellationToken);

    /// <summary>The locations where the caller works as an employee of the organization.</summary>
    Task<ApiResult<IReadOnlyList<EmployeeLocationResponse>>> GetMyEmployeeLocationsAsync(
        Guid organizationId, CancellationToken cancellationToken);
}