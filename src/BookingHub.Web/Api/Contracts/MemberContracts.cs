namespace BookingHub.Web.Api.Contracts;

public sealed record OrganizationMemberResponse(
    Guid OrganizationMemberId, Guid UserId, string? Email, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);

public sealed record AddOrganizationMemberByEmailRequest(string? Email, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);

public sealed record ChangeOrganizationMemberRoleRequest(OrganizationRole NewRole, Guid? LocationId, Guid? EmployeeId);