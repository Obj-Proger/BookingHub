namespace BookingHub.Application.Features.Employees.DTOs;

public sealed record EmployeeAssignmentResponse(Guid AssignmentId, Guid LocationId, string LocationName);