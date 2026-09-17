using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.DTOs;

public sealed record MyOrganizationMembershipResponse(
    Guid OrganizationId, string OrganizationName, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);