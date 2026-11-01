using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Common.Notifications;

public class GuestMessagesTests
{
    private readonly IGuestNotifier _notifier = Substitute.For<IGuestNotifier>();
    private readonly IWebLinkBuilder _linkBuilder = Substitute.For<IWebLinkBuilder>();
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();

    private GuestMessages CreateSut() => new(_notifier, _linkBuilder, _organizationRepository, _locationRepository);

    private WaitlistEntry CreateOfferedEntry()
    {
        var location = Location.Create(
            Guid.CreateVersion7(), "Downtown", Address.Create("221B Baker Street").Value, "UTC",
            WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value).Value;
        _locationRepository.GetByIdAsync(location.OrganizationId, location.Id, Arg.Any<CancellationToken>()).Returns(location);
        _organizationRepository.GetSlugAsync(location.OrganizationId, Arg.Any<CancellationToken>()).Returns("acme");

        var entry = WaitlistEntry.Create(
            location.OrganizationId, location.Id, null, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value).Value,
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;
        entry.Offer(
            Guid.CreateVersion7(), TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value,
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        return entry;
    }

    [Fact]
    public async Task WaitlistSlotOffered_SendsAbsoluteLinkToTheOfferPage()
    {
        var entry = CreateOfferedEntry();
        var expectedPath = $"/book/acme/waitlist/{entry.Id}?token={entry.ManagementToken.Value}";
        _linkBuilder.Build(expectedPath).Returns($"https://app.test{expectedPath}");
        var sut = CreateSut();

        await sut.WaitlistSlotOfferedAsync(entry, CancellationToken.None);

        _notifier.Received(1).Notify(
            entry.OrganizationId, entry.ClientContact, Arg.Any<string>(),
            Arg.Is<string>(body => body!.Contains($"https://app.test{expectedPath}")));
    }
}