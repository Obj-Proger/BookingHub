internal sealed class UnhideReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UnhideReviewCommand>
{
    public async Task<Result> Handle(UnhideReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(command.LocationId, command.ReviewId, cancellationToken);
        if (review is null)
            return Result.Failure(ApplicationErrors.Review.NotFound);

        review.Unhide();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}