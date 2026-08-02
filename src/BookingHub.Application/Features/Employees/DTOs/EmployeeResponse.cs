namespace BookingHub.Application.Features.Employees.DTOs;

public sealed record EmployeeResponse(Guid EmployeeId, string FullName, string? PhotoUrl, bool IsBookable);