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
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly ISmsService _smsService = Substitute.For<ISmsService>();

    private WaitlistSlotOfferedNotificationHandler CreateSut() => new(_waitlistEntryRepository, _emailService, _smsService);

    private static WaitlistEntry CreateOfferedEntry(Email? email = null)
    {
        var entry = WaitlistEntry.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value, email: email),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;
        entry.Offer(
            Guid.CreateVersion7(), TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value,
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        return entry;
    }

    [Fact]
    public async Task Handle_EntryHasEmail_SendsBothEmailAndSms()
    {
        var entry = CreateOfferedEntry(Email.Create("guest@example.com").Value);
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var domainEvent = new WaitlistSlotOfferedEvent(
            entry.Id, entry.OrganizationId, entry.ClientContact, entry.OfferedSlot!, entry.OfferExpiresAtUtc!.Value, DateTime.UtcNow);
        var sut = CreateSut();

        await sut.Handle(domainEvent, CancellationToken.None);

        await _emailService.Received(1).SendAsync(Arg.Is<EmailMessage>(m => m.ToAddress == "guest@example.com"), Arg.Any<CancellationToken>());
        await _smsService.Received(1).SendAsync(Arg.Any<SmsMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EntryHasNoEmail_SendsOnlySms()
    {
        var entry = CreateOfferedEntry();
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var domainEvent = new WaitlistSlotOfferedEvent(
            entry.Id, entry.OrganizationId, entry.ClientContact, entry.OfferedSlot!, entry.OfferExpiresAtUtc!.Value, DateTime.UtcNow);
        var sut = CreateSut();

        await sut.Handle(domainEvent, CancellationToken.None);

        await _emailService.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        await _smsService.Received(1).SendAsync(Arg.Any<SmsMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EntryNoLongerExists_SendsNothing()
    {
        _waitlistEntryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WaitlistEntry?)null);
        var domainEvent = new WaitlistSlotOfferedEvent(
            Guid.CreateVersion7(), Guid.CreateVersion7(), ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        var sut = CreateSut();

        await sut.Handle(domainEvent, CancellationToken.None);

        await _emailService.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        await _smsService.DidNotReceive().SendAsync(Arg.Any<SmsMessage>(), Arg.Any<CancellationToken>());
    }
}