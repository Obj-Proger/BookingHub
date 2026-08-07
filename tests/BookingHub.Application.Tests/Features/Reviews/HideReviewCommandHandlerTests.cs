using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Reviews.Commands.HideReview;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Reviews;

public class HideReviewCommandHandlerTests
{
    private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid ReviewId = Guid.CreateVersion7();

    [Fact]
    public async Task Handle_ReviewFoundForThisLocation_Hides()
    {
        var review = Review.Create(Guid.CreateVersion7(), LocationId, Guid.CreateVersion7(), Guid.CreateVersion7(), 5, null, DateTime.UtcNow).Value;
        _reviewRepository.GetByIdAsync(LocationId, ReviewId, Arg.Any<CancellationToken>()).Returns(review);
        var sut = new HideReviewCommandHandler(_reviewRepository, _unitOfWork);

        var result = await sut.Handle(new HideReviewCommand(Guid.CreateVersion7(), LocationId, ReviewId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        review.IsHidden.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFoundForThisLocation_FailsWithNotFoundError()
    {
        // Same repository-level guarantee as EmployeeLocationAssignment/LocationServiceOverride —
        // wrong-location and nonexistent are indistinguishable here by design.
        _reviewRepository.GetByIdAsync(LocationId, ReviewId, Arg.Any<CancellationToken>()).Returns((Review?)null);
        var sut = new HideReviewCommandHandler(_reviewRepository, _unitOfWork);

        var result = await sut.Handle(new HideReviewCommand(Guid.CreateVersion7(), LocationId, ReviewId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Review.NotFound);
    }
}