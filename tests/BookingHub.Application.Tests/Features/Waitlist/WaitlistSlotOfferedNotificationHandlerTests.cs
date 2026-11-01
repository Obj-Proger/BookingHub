using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist.EventHandlers;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Events;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class WaitlistSlotOfferedNotificationHandlerTests
{
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IGuestMessages _guestMessages = Substitute.For<IGuestMessages>();

    private WaitlistSlotOfferedNotificationHandler CreateSut() => new(_waitlistEntryRepository, _guestMessages);

    private static WaitlistEntry CreateOfferedEntry()
    {
        var entry = WaitlistEntry.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value).Value,
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;
        entry.Offer(
            Guid.CreateVersion7(), TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value,
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        return entry;
    }

    private static WaitlistSlotOfferedEvent CreateEvent(WaitlistEntry entry) =>
        new(entry.Id, entry.OrganizationId, entry.ClientContact, entry.OfferedSlot!, entry.OfferExpiresAtUtc!.Value, DateTime.UtcNow);

    [Fact]
    public async Task Handle_EntryStillOffered_TellsTheGuest()
    {
        var entry = CreateOfferedEntry();
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        await sut.Handle(CreateEvent(entry), CancellationToken.None);

        await _guestMessages.Received(1).WaitlistSlotOfferedAsync(entry, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OfferNoLongerOpen_SendsNothing()
    {
        var entry = CreateOfferedEntry();
        var domainEvent = CreateEvent(entry);
        entry.Cancel(DateTime.UtcNow);
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        await sut.Handle(domainEvent, CancellationToken.None);

        await _guestMessages.DidNotReceive().WaitlistSlotOfferedAsync(Arg.Any<WaitlistEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EntryNoLongerExists_SendsNothing()
    {
        _waitlistEntryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WaitlistEntry?)null);
        var domainEvent = new WaitlistSlotOfferedEvent(
            Guid.CreateVersion7(), Guid.CreateVersion7(), ClientContact.Create(PhoneNumber.Create("+14155552671").Value).Value,
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        var sut = CreateSut();

        await sut.Handle(domainEvent, CancellationToken.None);

        await _guestMessages.DidNotReceive().WaitlistSlotOfferedAsync(Arg.Any<WaitlistEntry>(), Arg.Any<CancellationToken>());
    }
}