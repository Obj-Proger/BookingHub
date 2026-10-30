namespace BookingHub.Application.Features.Reviews.DTOs;

/// <param name="VisitStartUtc">Start of the booking that was reviewed.</param>
/// <param name="CreatedAtUtc">When the guest submitted the review.</param>
public sealed record ReviewModerationResponse(
    Guid ReviewId, Guid EmployeeId, string EmployeeName, string ServiceName, DateTime VisitStartUtc,
    int Rating, string? Comment, bool IsHidden, DateTime CreatedAtUtc);