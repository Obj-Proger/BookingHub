namespace BookingHub.Application.Features.Organizations.DTOs;

public sealed record OrganizationResponse(
    Guid OrganizationId, string Name, string Slug, int CancellationDeadlineHours,
    TimeSpan PendingConfirmationWindow, TimeSpan AutoCompleteWindow, TimeSpan WaitlistOfferWindow);