using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;
public sealed record TransitionBookingsToAwaitingReviewCommand : ICommand<int>;
