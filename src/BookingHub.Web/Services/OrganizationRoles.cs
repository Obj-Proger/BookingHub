using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

internal static class OrganizationRoles
{
    /// <summary>Roles that manage the whole organization; the API enforces the same rule.</summary>
    public static readonly OrganizationRole[] Management = [OrganizationRole.Owner, OrganizationRole.Administrator];

    /// <summary>Roles that manage at least one location.</summary>
    public static readonly OrganizationRole[] LocationManagement =
        [OrganizationRole.Owner, OrganizationRole.Administrator, OrganizationRole.LocationManager];

    public static bool CanManage(OrganizationRole role) => Management.Contains(role);

    /// <summary>Organization managers, and the location manager of exactly this location.</summary>
    public static bool CanManageLocation(MyOrganizationMembershipResponse membership, Guid locationId) =>
        CanManage(membership.Role)
        || (membership.Role == OrganizationRole.LocationManager && membership.LocationId == locationId);
}