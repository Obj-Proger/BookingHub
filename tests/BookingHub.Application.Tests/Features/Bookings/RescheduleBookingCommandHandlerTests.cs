using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;
using MockQueryable;

namespace BookingHub.Application.Tests.Features.Bookings;

public class RescheduleBookingCommandHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RescheduleBookingCommandHandler CreateSut() => new(_bookingRepository, _dbContext, _unitOfWork);

    [Fact]
    public async Task Handle_NonUtcNewStartTime_FailsWithNotUtcError()
    {
        var sut = CreateSut();
        var localTime = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1), DateTimeKind.Local);

        var result = await sut.Handle(new RescheduleBookingCommand(Guid.CreateVersion7(), "any-token", localTime), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TimeSlot.NotUtc);
        await _bookingRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BookingNotFound_FailsWithNotFoundError()
    {
        _bookingRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Booking?)null);
        var sut = CreateSut();

        var result = await sut.Handle(
            new RescheduleBookingCommand(Guid.CreateVersion7(), "any-token", DateTime.UtcNow.AddDays(1)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.NotFound);
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithoutCheckingDeadlineOrAvailability()
    {
        var organization = Organization.Create("Name", "slug").Value;
        var booking = CreateConfirmedBooking(organization.Id, TimeSpan.FromHours(48));
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = CreateSut();

        var result = await sut.Handle(
            new RescheduleBookingCommand(booking.Id, "wrong-token", DateTime.UtcNow.AddDays(2)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidManagementToken);
        _ = _dbContext.DidNotReceive().Organizations;
    }

    [Fact]
    public async Task Handle_DeadlinePassed_FailsWithCancellationDeadlinePassedError()
    {
        var organization = Organization.Create("Name", "slug").Value;
        var booking = CreateConfirmedBooking(organization.Id, TimeSpan.FromHours(2));
        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        _dbContext.Organizations.Returns(new[] { organization }.ToList().BuildMock());
        var sut = CreateSut();

        var result = await sut.Handle(
            new RescheduleBookingCommand(booking.Id, booking.CancellationToken.Value, DateTime.UtcNow.AddHours(3)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.CancellationDeadlinePassed);
    }

    private static Booking CreateConfirmedBooking(Guid organizationId, TimeSpan leadTime)
    {
        var booking = Booking.CreatePending(
            organizationId, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow + leadTime, DateTime.UtcNow + leadTime + TimeSpan.FromHours(1)).Value, Money.Create(50m, "USD").Value,
            BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);
        return booking;
    }
}