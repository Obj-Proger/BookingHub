using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.DTOs;

/// <param name="Email">Null only if the user account no longer exists.</param>
public sealed record OrganizationMemberResponse(
    Guid OrganizationMemberId, Guid UserId, string? Email, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);