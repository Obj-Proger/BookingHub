namespace BookingHub.Domain.Tests.Entities;

public class ReviewTests
{
    private static readonly Guid ValidOrganizationId = Guid.CreateVersion7();
    private static readonly Guid ValidEmployeeId = Guid.CreateVersion7();
    private static readonly Guid ValidBookingId = Guid.CreateVersion7();
    private static readonly DateTime UtcNow = new(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);

    private static Result<Review> CreateValidReview(int rating = 5, string? comment = "Great service!") =>
        Review.Create(ValidOrganizationId, Guid.CreateVersion7(), ValidEmployeeId, ValidBookingId, rating, comment, UtcNow);

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var result = CreateValidReview();

        result.IsSuccess.Should().BeTrue();
        result.Value.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Great service!");
        result.Value.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void Create_ValidData_RaisesReviewSubmittedEvent()
    {
        var review = CreateValidReview().Value;

        review.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ReviewSubmittedEvent>();
    }

    [Fact]
    public void Create_NoComment_LeavesCommentNull()
    {
        var result = CreateValidReview(comment: null);

        result.Value.Comment.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = Review.Create(Guid.Empty, ValidEmployeeId, Guid.CreateVersion7(), ValidBookingId, 5, null, UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_EmptyBookingId_FailsWithValidationError()
    {
        var result = Review.Create(ValidOrganizationId, Guid.CreateVersion7(), ValidEmployeeId, Guid.Empty, 5, null, UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_RatingOutOfRange_FailsWithRatingOutOfRangeError(int rating)
    {
        var result = CreateValidReview(rating: rating);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Review.RatingOutOfRange);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_BoundaryRatings_Succeed(int rating)
    {
        var result = CreateValidReview(rating: rating);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_CommentExceedingMaximumLength_FailsWithCommentTooLongError()
    {
        var tooLong = new string('a', 2001);

        var result = CreateValidReview(comment: tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Review.CommentTooLong);
    }

    [Fact]
    public void Create_CommentWithSurroundingWhitespace_IsTrimmed()
    {
        var result = CreateValidReview(comment: "  Great service!  ");

        result.Value.Comment.Should().Be("Great service!");
    }

    [Fact]
    public void Hide_SetsIsHiddenToTrue()
    {
        var review = CreateValidReview().Value;

        review.Hide();

        review.IsHidden.Should().BeTrue();
    }

    [Fact]
    public void Unhide_SetsIsHiddenToFalse()
    {
        var review = CreateValidReview().Value;
        review.Hide();

        review.Unhide();

        review.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void Create_EmptyLocationId_FailsWithValidationError()
    {
        var result = Domain.Entities.Review.Create(ValidOrganizationId, Guid.Empty, ValidEmployeeId, ValidBookingId, 5, null, UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}