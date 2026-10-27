using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <param name="Segment">Path below /app/{organizationId}; empty for the organization's start page.</param>
/// <param name="Roles">Roles that see the item. The API enforces access independently.</param>
internal sealed record StaffNavItem(string Title, string Segment, OrganizationRole[] Roles);

internal static class StaffNavigation
{
    private static readonly OrganizationRole[] AllRoles = Enum.GetValues<OrganizationRole>();

    private static readonly StaffNavItem[] Items =
    [
        new("Overview", "", AllRoles),
        new("Bookings", "bookings", AllRoles),
        new("Clients", "clients", AllRoles),
        new("Locations", "locations", OrganizationRoles.LocationManagement),
        new("Services", "services", OrganizationRoles.LocationManagement),
        new("Employees", "employees", OrganizationRoles.LocationManagement),
        new("Settings", "settings", OrganizationRoles.Management)
    ];

    public static IEnumerable<StaffNavItem> ItemsFor(OrganizationRole role) =>
        Items.Where(item => item.Roles.Contains(role));

    public static string Href(Guid organizationId, StaffNavItem item) =>
        item.Segment.Length == 0
            ? StaffRoutes.Organization(organizationId)
            : $"{StaffRoutes.Organization(organizationId)}/{item.Segment}";
}