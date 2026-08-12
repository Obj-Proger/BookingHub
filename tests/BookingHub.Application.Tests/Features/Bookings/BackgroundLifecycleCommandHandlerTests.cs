using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;
using BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;
using BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Bookings;

public class BackgroundLifecycleCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static Booking CreatePendingBooking() => Booking.CreatePending(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
        ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
        TimeSlot.Create(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(3)).Value, Money.Create(50m, "USD").Value,
        BookingSource.Public, DateTime.UtcNow).Value;

    [Fact]
    public async Task ExpirePendingBookings_ExpiresEachReturnedBookingAndReportsCount()
    {
        var bookings = new[] { CreatePendingBooking(), CreatePendingBooking() };
        _bookingRepository.GetPendingBookingsPastConfirmationWindowAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(bookings);
        var sut = new ExpirePendingBookingsCommandHandler(_bookingRepository, _unitOfWork, TimeProvider.System);

        var result = await sut.Handle(new ExpirePendingBookingsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        bookings.Should().OnlyContain(b => b.Status == BookingStatus.Expired);
    }

    [Fact]
    public async Task ExpirePendingBookings_NoneReturned_ReportsZeroWithoutFailing()
    {
        _bookingRepository.GetPendingBookingsPastConfirmationWindowAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);
        var sut = new ExpirePendingBookingsCommandHandler(_bookingRepository, _unitOfWork, TimeProvider.System);

        var result = await sut.Handle(new ExpirePendingBookingsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task TransitionBookingsToAwaitingReview_TransitionsEachReturnedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Confirm(DateTime.UtcNow);
        _bookingRepository.GetConfirmedBookingsWithEndedSlotsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([booking]);
        var timeProvider = new FixedTimeProvider(booking.TimeSlot.EndUtc.AddMinutes(1));
        var sut = new TransitionBookingsToAwaitingReviewCommandHandler(_bookingRepository, _unitOfWork, timeProvider);

        var result = await sut.Handle(new TransitionBookingsToAwaitingReviewCommand(), CancellationToken.None);

        result.Value.Should().Be(1);
        booking.Status.Should().Be(BookingStatus.AwaitingReview);
    }

    [Fact]
    public async Task AutoCompleteBookings_CompletesEachReturnedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Confirm(DateTime.UtcNow);
        booking.TransitionToAwaitingReview(DateTime.UtcNow.AddHours(4));
        _bookingRepository.GetAwaitingReviewBookingsPastAutoCompleteWindowAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([booking]);
        var sut = new AutoCompleteBookingsCommandHandler(_bookingRepository, _unitOfWork, TimeProvider.System);

        var result = await sut.Handle(new AutoCompleteBookingsCommand(), CancellationToken.None);

        result.Value.Should().Be(1);
        booking.Status.Should().Be(BookingStatus.Completed);
    }
}