using BookingHub.Domain.Events;

namespace BookingHub.Domain.Entities;

public sealed class Review : BaseEntity, IAuditable
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MaxCommentLength = 2000;

    public Guid OrganizationId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid BookingId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsHidden { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedAtUtc { get; private set; }

    private Review(
        Guid id, Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, int rating, string? comment)
        : base(id)
    {
        OrganizationId = organizationId;
        LocationId = locationId;
        EmployeeId = employeeId;
        BookingId = bookingId;
        Rating = rating;
        Comment = comment;
    }

    private Review()
    {
    }

    /// <remarks>
    /// Does not verify that <paramref name="bookingId"/> refers to a completed booking —
    /// that requires loading the booking, which is the Application layer's responsibility.
    /// </remarks>
    public static Result<Review> Create(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, int rating, string? comment, DateTime utcNow)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, DomainErrors.Review.OrganizationIdEmpty);
        if (organizationIdResult.IsFailure)
            return Result.Failure<Review>(organizationIdResult.Error);

        var locationIdResult = Guard.NotEmpty(locationId, DomainErrors.Review.LocationIdEmpty);
        if (locationIdResult.IsFailure)
            return Result.Failure<Review>(locationIdResult.Error);

        var employeeIdResult = Guard.NotEmpty(employeeId, DomainErrors.Review.EmployeeIdEmpty);
        if (employeeIdResult.IsFailure)
            return Result.Failure<Review>(employeeIdResult.Error);

        var bookingIdResult = Guard.NotEmpty(bookingId, DomainErrors.Review.BookingIdEmpty);
        if (bookingIdResult.IsFailure)
            return Result.Failure<Review>(bookingIdResult.Error);

        if (rating is < MinRating or > MaxRating)
            return Result.Failure<Review>(DomainErrors.Review.RatingOutOfRange);

        var commentResult = Guard.OptionalText(comment, MaxCommentLength, DomainErrors.Review.CommentTooLong);
        if (commentResult.IsFailure)
            return Result.Failure<Review>(commentResult.Error);

        var review = new Review(Guid.CreateVersion7(), organizationId, locationId, employeeId, bookingId, rating, commentResult.Value);
        review.RaiseDomainEvent(new ReviewSubmittedEvent(review.Id, organizationId, employeeId, rating, utcNow));
        return review;
    }

    public void Hide() => IsHidden = true;
    public void Unhide() => IsHidden = false;
}