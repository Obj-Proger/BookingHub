using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Waitlist.Commands.LeaveWaitlist;

/// <summary>Anonymous by design — same token as <c>ConfirmWaitlistOfferCommand</c>.</summary>
public sealed record LeaveWaitlistCommand(Guid WaitlistEntryId, string? Token) : ICommand;