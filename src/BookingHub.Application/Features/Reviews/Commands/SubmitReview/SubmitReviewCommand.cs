using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Reviews.DTOs;

namespace BookingHub.Application.Features.Reviews.Commands.SubmitReview;

/// <summary>Anonymous by design — reached via the link sent when a booking becomes Completed
/// authenticated by the booking's own management token.</summary>
public sealed record SubmitReviewCommand(Guid BookingId, string? Token, int Rating, string? Comment)
    : ICommand<ReviewSubmittedResponse>;