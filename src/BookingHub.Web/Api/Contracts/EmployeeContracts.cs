namespace BookingHub.Web.Api.Contracts;

public sealed record EmployeeResponse(Guid EmployeeId, string FullName, string? PhotoUrl, bool IsBookable);

public sealed record EmployeeCreatedResponse(Guid EmployeeId, string FullName);

public sealed record EmployeeAssignmentResponse(Guid AssignmentId, Guid LocationId, string LocationName);

public sealed record EmployeeAssignmentCreatedResponse(Guid AssignmentId, Guid EmployeeId, Guid LocationId);

public sealed record CreateEmployeeRequest(string? FullName);

public sealed record RenameEmployeeRequest(string? NewFullName);

public sealed record SetEmployeeBookableRequest(bool IsBookable);

public sealed record UpdateEmployeePhotoRequest(string? PhotoUrl);

public sealed record AssignEmployeeToLocationRequest(Guid EmployeeId);