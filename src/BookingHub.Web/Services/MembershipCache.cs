using BookingHub.Web.Api;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <summary>Holds the signed-in user's organization memberships for the lifetime of one circuit.</summary>
internal sealed class MembershipCache(IMeApiClient meApiClient)
{
    private IReadOnlyList<MyOrganizationMembershipResponse>? _memberships;

    public async Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        if (_memberships is not null)
            return ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>.Success(_memberships);

        var result = await meApiClient.GetMyOrganizationsAsync(cancellationToken);
        if (result.IsSuccess)
            _memberships = result.Value;

        return result;
    }

    public void Invalidate() => _memberships = null;
}