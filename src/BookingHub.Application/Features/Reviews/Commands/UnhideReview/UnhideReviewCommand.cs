using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Reviews.Commands.UnhideReview;

public sealed record UnhideReviewCommand(Guid OrganizationId, Guid LocationId, Guid ReviewId) : ICommand, IRequireLocationManagement;