using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist.Commands.LeaveWaitlist;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class LeaveWaitlistCommandHandlerTests
{
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LeaveWaitlistCommandHandler CreateSut() => new(_waitlistEntryRepository, _unitOfWork);

    private static WaitlistEntry CreateWaitingEntry() => WaitlistEntry.Create(
        Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(),
        ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
        TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;

    [Fact]
    public async Task Handle_CorrectToken_CancelsEntry()
    {
        var entry = CreateWaitingEntry();
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        var result = await sut.Handle(new LeaveWaitlistCommand(entry.Id, entry.ManagementToken.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_EntryOffered_StillSucceeds_AsDecliningTheOffer()
    {
        var entry = CreateWaitingEntry();
        entry.Offer(Guid.CreateVersion7(), entry.DesiredWindow, DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        var result = await sut.Handle(new LeaveWaitlistCommand(entry.Id, entry.ManagementToken.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithInvalidManagementTokenError()
    {
        var entry = CreateWaitingEntry();
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        var result = await sut.Handle(new LeaveWaitlistCommand(entry.Id, "wrong-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.WaitlistEntry.InvalidManagementToken);
    }
}