using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.DTOs;

public sealed record OrganizationMemberResponse(
    Guid OrganizationMemberId, Guid UserId, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);