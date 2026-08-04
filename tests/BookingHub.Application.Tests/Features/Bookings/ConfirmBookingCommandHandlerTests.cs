using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Bookings;

public class ConfirmBookingCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ConfirmBookingCommandHandler CreateSut() => new(_bookingRepository, _unitOfWork);

    private static Booking CreatePendingBooking() => Booking.CreatePending(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
        ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
        TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value,
        BookingSource.Public, DateTime.UtcNow).Value;

    [Fact]
    public async Task Handle_CorrectToken_ConfirmsBooking()
    {
        var booking = CreatePendingBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmBookingCommand(booking.Id, booking.ConfirmationToken.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithoutChangingStatus()
    {
        var booking = CreatePendingBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmBookingCommand(booking.Id, "wrong-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidConfirmationToken);
        booking.Status.Should().Be(BookingStatus.Pending);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingToken_FailsWithInvalidConfirmationTokenError()
    {
        var booking = CreatePendingBooking();
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmBookingCommand(booking.Id, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidConfirmationToken);
    }

    [Fact]
    public async Task Handle_BookingNotFound_FailsWithNotFoundError()
    {
        _bookingRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Booking?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmBookingCommand(Guid.CreateVersion7(), "any-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.NotFound);
    }

    [Fact]
    public async Task Handle_AlreadyConfirmed_FailsWithDomainCannotConfirmError()
    {
        var booking = CreatePendingBooking();
        booking.Confirm(DateTime.UtcNow);
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmBookingCommand(booking.Id, booking.ConfirmationToken.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotConfirm);
    }
}