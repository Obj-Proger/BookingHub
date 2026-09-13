namespace BookingHub.API.Controllers.Waitlist;

public sealed record JoinWaitlistRequest(
    Guid LocationId, Guid? EmployeeId, Guid ServiceId, DateTime DesiredStartUtc, DateTime DesiredEndUtc,
    string? Phone, string? ClientName, string? ClientEmail);

public sealed record ConfirmWaitlistOfferRequest(string? Token);
public sealed record LeaveWaitlistRequest(string? Token);