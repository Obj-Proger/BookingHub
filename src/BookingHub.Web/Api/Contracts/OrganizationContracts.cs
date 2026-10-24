namespace BookingHub.Web.Api.Contracts;

public sealed record CreateOrganizationRequest(string? Name, string? Slug);

public sealed record OrganizationCreatedResponse(Guid OrganizationId, string Name, string Slug);

public sealed record OrganizationResponse(
    Guid OrganizationId, string Name, string Slug, TimeSpan CancellationDeadline,
    TimeSpan PendingConfirmationWindow, TimeSpan AutoCompleteWindow, TimeSpan WaitlistOfferWindow,
    bool CanAdministratorsViewFinancials);

public sealed record RenameOrganizationRequest(string? NewName);

public sealed record UpdateCancellationDeadlineRequest(TimeSpan Deadline);

public sealed record UpdateSchedulingWindowRequest(TimeSpan Window);

public sealed record SetAdministratorFinancialAccessRequest(bool Enabled);