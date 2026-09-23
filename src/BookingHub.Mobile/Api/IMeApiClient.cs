using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

public interface IMeApiClient
{
    Task<IReadOnlyList<MyOrganizationMembershipResponse>?> GetMyOrganizationsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<EmployeeLocationResponse>?> GetMyLocationsAsync(Guid organizationId, CancellationToken cancellationToken);
}