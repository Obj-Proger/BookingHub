using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IMeApiClient
{
    Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetMyOrganizationsAsync(CancellationToken cancellationToken);
}