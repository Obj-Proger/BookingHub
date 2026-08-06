using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Waitlist;

internal interface IWaitlistOfferService
{
    Task TryOfferFreedSlotAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, TimeSlot freedSlot,
        CancellationToken cancellationToken);
}