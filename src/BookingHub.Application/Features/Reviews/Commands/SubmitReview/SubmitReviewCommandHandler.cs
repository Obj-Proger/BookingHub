using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Reviews.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Reviews.Commands.SubmitReview;

internal sealed class SubmitReviewCommandHandler(
    IBookingRepository bookingRepository, IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SubmitReviewCommand, ReviewSubmittedResponse>
{
    public async Task<Result<ReviewSubmittedResponse>> Handle(SubmitReviewCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure<ReviewSubmittedResponse>(ApplicationErrors.Booking.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!booking.CancellationToken.Matches(providedToken))
            return Result.Failure<ReviewSubmittedResponse>(ApplicationErrors.Booking.InvalidManagementToken);

        if (booking.Status != BookingStatus.Completed)
            return Result.Failure<ReviewSubmittedResponse>(ApplicationErrors.Review.BookingNotCompleted);

        if (await reviewRepository.ExistsForBookingAsync(booking.Id, cancellationToken))
            return Result.Failure<ReviewSubmittedResponse>(ApplicationErrors.Review.AlreadyExists);

        var reviewResult = Review.Create(
            booking.OrganizationId, booking.LocationId, booking.EmployeeId, booking.Id,
            command.Rating, command.Comment, DateTime.UtcNow);
        if (reviewResult.IsFailure)
            return Result.Failure<ReviewSubmittedResponse>(reviewResult.Error);

        reviewRepository.Add(reviewResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewSubmittedResponse(reviewResult.Value.Id);
    }
}