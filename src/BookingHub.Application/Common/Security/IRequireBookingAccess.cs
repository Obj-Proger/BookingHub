namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request restricted to organization-wide managers (Owner/Administrator), a
/// LocationManager scoped to <see cref="LocationId"/>, OR the specific Employee who owns
/// the booking (<see cref="EmployeeId"/>) — matching the Vision Document's rule that staff
/// may act on Completed/NoShow only for their own bookings.
/// </summary>
public interface IRequireBookingAccess : IRequireOrganizationMembership
{
    Guid LocationId { get; }
    Guid EmployeeId { get; }
}