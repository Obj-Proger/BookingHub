using BookingHub.Web.Api;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <summary>Holds the signed-in user's organization memberships for the lifetime of one circuit.</summary>
internal sealed class MembershipCache(IMeApiClient meApiClient)
{
    private IReadOnlyList<MyOrganizationMembershipResponse>? _memberships;

    /// <summary>Raised when a cached membership was modified in place.</summary>
    public event Action? Changed;

    public async Task<ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        if (_memberships is not null)
            return ApiResult<IReadOnlyList<MyOrganizationMembershipResponse>>.Success(_memberships);

        var result = await meApiClient.GetMyOrganizationsAsync(cancellationToken);
        if (result.IsSuccess)
            _memberships = result.Value;

        return result;
    }

    /// <summary>Drops the cache so the next read reloads the memberships from the API.</summary>
    public void Invalidate() => _memberships = null;

    public void RenameOrganization(Guid organizationId, string newName)
    {
        if (_memberships is null)
            return;

        _memberships =
        [
            .. _memberships.Select(membership => membership.OrganizationId == organizationId
                ? membership with { OrganizationName = newName }
                : membership)
        ];

        Changed?.Invoke();
    }
}