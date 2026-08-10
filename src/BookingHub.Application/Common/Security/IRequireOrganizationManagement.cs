namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request restricted to organization-wide managers (Owner or Administrator).
/// See <see cref="IRequireLocationManagement"/> for the location-scoped variant, which
/// additionally allows a LocationManager scoped to one specific location.
/// </summary>
public interface IRequireOrganizationManagement : IRequireOrganizationMembership;