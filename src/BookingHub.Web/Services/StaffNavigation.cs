using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <param name="TitleKey">Key of the item's title in the shared resources.</param>
/// <param name="Segment">Path below /app/{organizationId}; empty for the organization's start page.</param>
/// <param name="Roles">Roles that see the item. The API enforces access independently.</param>
internal sealed record StaffNavItem(string TitleKey, string Segment, OrganizationRole[] Roles);

internal static class StaffNavigation
{
    private static readonly OrganizationRole[] AllRoles = Enum.GetValues<OrganizationRole>();

    private static readonly StaffNavItem[] Items =
    [
        new("Nav_Overview", "", AllRoles),
        new("Nav_Bookings", "bookings", AllRoles),
        new("Nav_Clients", "clients", AllRoles),
        new("Nav_Analytics", "analytics", OrganizationRoles.LocationManagement),
        new("Nav_Reviews", "reviews", OrganizationRoles.LocationManagement),
        new("Nav_Locations", "locations", OrganizationRoles.LocationManagement),
        new("Nav_Services", "services", OrganizationRoles.LocationManagement),
        new("Nav_Employees", "employees", OrganizationRoles.LocationManagement),
        new("Nav_Members", "members", OrganizationRoles.Management),
        new("Nav_Settings", "settings", OrganizationRoles.Management)
    ];

    public static IEnumerable<StaffNavItem> ItemsFor(OrganizationRole role) =>
        Items.Where(item => item.Roles.Contains(role));

    public static string Href(Guid organizationId, StaffNavItem item) =>
        item.Segment.Length == 0
            ? StaffRoutes.Organization(organizationId)
            : $"{StaffRoutes.Organization(organizationId)}/{item.Segment}";
}