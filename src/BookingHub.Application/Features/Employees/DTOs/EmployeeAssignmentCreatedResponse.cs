namespace BookingHub.Application.Features.Employees.DTOs;

public sealed record EmployeeAssignmentCreatedResponse(Guid AssignmentId, Guid EmployeeId, Guid LocationId);