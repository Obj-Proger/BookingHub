using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.Commands.CancelBooking;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Bookings;

public class CancelBookingCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CancelBookingCommandHandler CreateSut() => new(_bookingRepository, _dbContext, _unitOfWork);

    private static Booking CreateConfirmedBooking(TimeSpan leadTime)
    {
        var booking = Booking.CreatePending(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow + leadTime, DateTime.UtcNow + leadTime + TimeSpan.FromHours(1)).Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        return booking;
    }

    private void SetUpOrganizations(IEnumerable<Organization> organizations) =>
        _dbContext.Organizations.Returns(organizations.AsQueryable());

    [Fact]
    public async Task Handle_CorrectTokenAndBeforeDeadline_CancelsBooking()
    {
        var organization = Organization.Create("Name", "slug").Value;
        var booking = CreateConfirmedBooking(organization.Id, TimeSpan.FromHours(48));
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        SetUpOrganizations([organization]);
        var sut = CreateSut();

        var result = await sut.Handle(new CancelBookingCommand(booking.Id, booking.CancellationToken.Value, "Changed plans"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_DeadlinePassed_FailsWithCancellationDeadlinePassedError()
    {
        var organization = Organization.Create("Name", "slug").Value;
        var booking = CreateConfirmedBooking(organization.Id, TimeSpan.FromHours(2)); // inside the 24h default deadline
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        SetUpOrganizations([organization]);
        var sut = CreateSut();

        var result = await sut.Handle(new CancelBookingCommand(booking.Id, booking.CancellationToken.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.CancellationDeadlinePassed);
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithInvalidManagementTokenError()
    {
        var organization = Organization.Create("Name", "slug").Value;
        var booking = CreateConfirmedBooking(organization.Id, TimeSpan.FromHours(48));
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(new CancelBookingCommand(booking.Id, "wrong-token", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidManagementToken);
    }

    private static Booking CreateConfirmedBooking(Guid organizationId, TimeSpan leadTime)
    {
        var booking = Booking.CreatePending(
            organizationId, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow + leadTime, DateTime.UtcNow + leadTime + TimeSpan.FromHours(1)).Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        return booking;
    }
}