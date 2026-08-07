using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Reviews.Commands.HideReview;

public sealed record HideReviewCommand(Guid OrganizationId, Guid LocationId, Guid ReviewId) : ICommand, IRequireLocationManagement;