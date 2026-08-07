using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Reviews.Commands.HideReview;

internal sealed class HideReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<HideReviewCommand>
{
    public async Task<Result> Handle(HideReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(command.LocationId, command.ReviewId, cancellationToken);
        if (review is null)
            return Result.Failure(ApplicationErrors.Review.NotFound);

        review.Hide();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}