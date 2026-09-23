using BookingHub.Mobile.Domain;

namespace BookingHub.Mobile.Api.Contracts;

public sealed record MyOrganizationMembershipResponse(
    Guid OrganizationId, string OrganizationName, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);