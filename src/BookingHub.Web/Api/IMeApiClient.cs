using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IMeApiClient
{
    /// <summary>Returns null when the call fails or the session is no longer valid.</summary>
    Task<IReadOnlyList<MyOrganizationMembershipResponse>?> GetMyOrganizationsAsync(CancellationToken cancellationToken);
}