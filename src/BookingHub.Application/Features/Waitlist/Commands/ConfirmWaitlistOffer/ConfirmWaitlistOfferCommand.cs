using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;

/// <summary>Anonymous by design — reached via the token link sent by <c>WaitlistSlotOfferedEvent</c>.</summary>
public sealed record ConfirmWaitlistOfferCommand(Guid WaitlistEntryId, string? Token) : ICommand<BookingCreatedResponse>;