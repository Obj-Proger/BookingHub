using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.Commands.MarkCompleted;
using BookingHub.Application.Features.Bookings.Commands.MarkNoShow;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Bookings;

public class MarkCompletedAndNoShowCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();
    private static readonly Guid BookingId = Guid.CreateVersion7();

    private static Booking CreateAwaitingReviewBooking()
    {
        var booking = Booking.CreatePending(
            OrganizationId, LocationId, EmployeeId, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)).Value, Money.Create(50m, "USD").Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        booking.TransitionToAwaitingReview(DateTime.UtcNow.AddHours(3));
        return booking;
    }

    [Fact]
    public async Task MarkCompleted_AwaitingReviewBookingFoundInScope_Completes()
    {
        _bookingRepository.GetByIdAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>())
            .Returns(CreateAwaitingReviewBooking());
        var sut = new MarkCompletedCommandHandler(_bookingRepository, _unitOfWork);

        var result = await sut.Handle(new MarkCompletedCommand(OrganizationId, LocationId, EmployeeId, BookingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MarkCompleted_NotFoundInScope_FailsWithNotFoundError()
    {
        // Whether it doesn't exist at all, or belongs to a different employee/location, the
        // repository's four-way filter (Commit — MarkCompleted/MarkNoShow) makes both cases
        // indistinguishable here by design — this test only needs to cover the null result.
        _bookingRepository.GetByIdAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);
        var sut = new MarkCompletedCommandHandler(_bookingRepository, _unitOfWork);

        var result = await sut.Handle(new MarkCompletedCommand(OrganizationId, LocationId, EmployeeId, BookingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.NotFound);
    }

    [Fact]
    public async Task MarkNoShow_AwaitingReviewBookingFoundInScope_MarksNoShow()
    {
        _bookingRepository.GetByIdAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>())
            .Returns(CreateAwaitingReviewBooking());
        var sut = new MarkNoShowCommandHandler(_bookingRepository, _unitOfWork);

        var result = await sut.Handle(new MarkNoShowCommand(OrganizationId, LocationId, EmployeeId, BookingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MarkNoShow_BookingStillConfirmed_FailsWithDomainCannotMarkNoShowError()
    {
        var booking = Booking.CreatePending(
            OrganizationId, LocationId, EmployeeId, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)).Value, Money.Create(50m, "USD").Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        _bookingRepository.GetByIdAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = new MarkNoShowCommandHandler(_bookingRepository, _unitOfWork);

        var result = await sut.Handle(new MarkNoShowCommand(OrganizationId, LocationId, EmployeeId, BookingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotMarkNoShow);
    }
}