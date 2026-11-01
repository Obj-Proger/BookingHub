using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;
/// <summary>Moves confirmed bookings whose slot has ended to awaiting review; sent by a background job.</summary>
public sealed record TransitionBookingsToAwaitingReviewCommand(Guid OrganizationId) : ICommand<int>, ISystemRequest;
