namespace BookingHub.Application.Features.Employees.DTOs;

public sealed record LocationAssignmentResponse(Guid AssignmentId, Guid EmployeeId, string EmployeeName);