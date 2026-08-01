namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request restricted to organization-wide managers (Owner/Administrator) OR
/// a LocationManager specifically scoped to <see cref="LocationId"/>.
/// </summary>
public interface IRequireLocationManagement : IRequireOrganizationMembership
{
    Guid LocationId { get; }
}