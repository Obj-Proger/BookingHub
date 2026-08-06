using BookingHub.Application.Features.Waitlist;
using BookingHub.Application.Features.Waitlist.EventHandlers;
using BookingHub.Domain.Events;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class WaitlistOfferEventHandlerTests
{
    private readonly IWaitlistOfferService _offerService = Substitute.For<IWaitlistOfferService>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();
    private static readonly Guid ServiceId = Guid.CreateVersion7();
    private static readonly TimeSlot Slot = TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value;

    [Fact]
    public async Task BookingCancelledWaitlistOfferHandler_MapsEventFieldsToService()
    {
        var domainEvent = new BookingCancelledEvent(Guid.CreateVersion7(), OrganizationId, LocationId, EmployeeId, ServiceId, Slot, DateTime.UtcNow);
        var sut = new BookingCancelledWaitlistOfferHandler(_offerService);

        await sut.Handle(domainEvent, CancellationToken.None);

        await _offerService.Received(1).TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, Slot, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BookingExpiredWaitlistOfferHandler_MapsEventFieldsToService()
    {
        var domainEvent = new BookingExpiredEvent(Guid.CreateVersion7(), OrganizationId, LocationId, EmployeeId, ServiceId, Slot, DateTime.UtcNow);
        var sut = new BookingExpiredWaitlistOfferHandler(_offerService);

        await sut.Handle(domainEvent, CancellationToken.None);

        await _offerService.Received(1).TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, Slot, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WaitlistOfferExpiredHandler_MapsOfferedEmployeeIdAndOfferedSlotToService()
    {
        var domainEvent = new WaitlistOfferExpiredEvent(Guid.CreateVersion7(), OrganizationId, LocationId, ServiceId, EmployeeId, Slot, DateTime.UtcNow);
        var sut = new WaitlistOfferExpiredHandler(_offerService);

        await sut.Handle(domainEvent, CancellationToken.None);

        await _offerService.Received(1).TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, Slot, Arg.Any<CancellationToken>());
    }
}