using BookingHub.Domain.Enums;

namespace BookingHub.API.Controllers.Organizations;

public sealed record CreateOrganizationRequest(string? Name, string? Slug);
public sealed record RenameOrganizationRequest(string? NewName);

public sealed record AddOrganizationMemberRequest(Guid UserId, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId);
public sealed record ChangeOrganizationMemberRoleRequest(OrganizationRole NewRole, Guid? LocationId, Guid? EmployeeId);

/// <param name="Deadline">.NET TimeSpan format only (e.g. "1.00:00:00" for one day, "02:30:00"
/// for two and a half hours) — ISO 8601 durations ("P1D") are not accepted.</param>
public sealed record UpdateCancellationDeadlineRequest(TimeSpan Deadline);

/// <param name="Window">Same .NET TimeSpan format as <see cref="UpdateCancellationDeadlineRequest"/>.</param>
public sealed record UpdateSchedulingWindowRequest(TimeSpan Window);

public sealed record SetAdministratorFinancialAccessRequest(bool Enabled);