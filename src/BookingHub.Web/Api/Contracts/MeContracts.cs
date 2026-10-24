namespace BookingHub.Web.Api.Contracts;

public enum OrganizationRole
{
    Owner = 1,
    Administrator = 2,
    LocationManager = 3,
    Employee = 4
}

public sealed record MyOrganizationMembershipResponse(
    Guid OrganizationId, string OrganizationName, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);