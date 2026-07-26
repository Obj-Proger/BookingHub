namespace BookingHub.Domain.Enums;

/// <summary>
/// A member's role within an organization, determining the scope and level
/// of access to organization data.
/// </summary>
public enum OrganizationRole
{
    /// <summary>Full access to the entire organization, including billing.</summary>
    Owner = 1,

    /// <summary>Manages staff, services, and schedules across all locations.</summary>
    Administrator = 2,

    /// <summary>Same as <see cref="Administrator"/>, but scoped to a single location.</summary>
    LocationManager = 3,

    /// <summary>Manages only their own bookings and clients.</summary>
    Employee = 4
}