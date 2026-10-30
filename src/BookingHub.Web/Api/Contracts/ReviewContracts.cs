namespace BookingHub.Web.Api.Contracts;

public sealed record ReviewModerationResponse(
    Guid ReviewId, Guid EmployeeId, string EmployeeName, string ServiceName, DateTime VisitStartUtc,
    int Rating, string? Comment, bool IsHidden, DateTime CreatedAtUtc);