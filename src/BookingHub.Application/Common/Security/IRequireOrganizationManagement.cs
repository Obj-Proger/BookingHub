namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request restricted to organization-wide managers (Owner or Administrator).
/// A location-scoped variant (allowing a LocationManager scoped to one specific location)
/// will be added once a feature that actually needs that distinction exists — Locations/Employees, next.
/// </summary>
public interface IRequireOrganizationManagement : IRequireOrganizationMembership;