using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Reviews.Commands.SubmitReview;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Reviews;

public class SubmitReviewCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IReviewRepository _reviewRepository = Substitute.For<IReviewRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private SubmitReviewCommandHandler CreateSut() => new(_bookingRepository, _reviewRepository, _unitOfWork);

    private static Booking CreateCompletedBooking()
    {
        var booking = Booking.CreatePending(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2)).Value,
            BookingSource.Public, DateTime.UtcNow.AddHours(-4)).Value;
        booking.Confirm(DateTime.UtcNow.AddHours(-4));
        booking.TransitionToAwaitingReview(DateTime.UtcNow.AddHours(-2));
        booking.Complete(DateTime.UtcNow.AddHours(-1));
        return booking;
    }

    [Fact]
    public async Task Handle_CompletedBookingCorrectTokenNoExistingReview_SubmitsReview()
    {
        var booking = CreateCompletedBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        _reviewRepository.ExistsForBookingAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new SubmitReviewCommand(booking.Id, booking.CancellationToken.Value, 5, "Great!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _reviewRepository.Received(1).Add(Arg.Is<Review>(
            r => r.OrganizationId == booking.OrganizationId && r.LocationId == booking.LocationId && r.EmployeeId == booking.EmployeeId));
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithoutCheckingBookingStatus()
    {
        var booking = CreateCompletedBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new SubmitReviewCommand(booking.Id, "wrong-token", 5, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidManagementToken);
        await _reviewRepository.DidNotReceive().ExistsForBookingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BookingNotCompleted_FailsWithBookingNotCompletedError()
    {
        var booking = Booking.CreatePending(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)).Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new SubmitReviewCommand(booking.Id, booking.CancellationToken.Value, 5, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Review.BookingNotCompleted);
    }

    [Fact]
    public async Task Handle_ReviewAlreadyExistsForBooking_FailsWithAlreadyExistsError()
    {
        var booking = CreateCompletedBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        _reviewRepository.ExistsForBookingAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(new SubmitReviewCommand(booking.Id, booking.CancellationToken.Value, 5, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Review.AlreadyExists);
    }

    [Fact]
    public async Task Handle_InvalidRating_FailsWithDomainRatingOutOfRangeError()
    {
        var booking = CreateCompletedBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        _reviewRepository.ExistsForBookingAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new SubmitReviewCommand(booking.Id, booking.CancellationToken.Value, 6, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Review.RatingOutOfRange);
    }
}