namespace BookingHub.Web.Api.Contracts;

public sealed record JoinWaitlistRequest(
    Guid LocationId, Guid? EmployeeId, Guid ServiceId, DateTime DesiredStartUtc, DateTime DesiredEndUtc,
    string? Phone, string? ClientName, string? ClientEmail);

public sealed record WaitlistEntryCreatedResponse(Guid WaitlistEntryId);

public sealed record ConfirmWaitlistOfferRequest(string? Token);
public sealed record LeaveWaitlistRequest(string? Token);