using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

internal static class OrganizationRoles
{
    /// <summary>Roles that manage the whole organization; the API enforces the same rule.</summary>
    public static readonly OrganizationRole[] Management = [OrganizationRole.Owner, OrganizationRole.Administrator];

    public static bool CanManage(OrganizationRole role) => Management.Contains(role);
}