using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Waitlist;

/// <summary>
/// Shared by both <c>BookingCancelledEvent</c> and <c>BookingExpiredEvent</c> handlers — a freed
/// slot is offered to the first matching waiting entry, trying the next one if the first no
/// longer qualifies (e.g. the freed slot has since slipped into the past).
/// </summary>
internal sealed class WaitlistOfferService(
    IWaitlistEntryRepository waitlistEntryRepository, IApplicationDbContext dbContext, IUnitOfWork unitOfWork)
{
    public async Task TryOfferFreedSlotAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, TimeSlot freedSlot,
        CancellationToken cancellationToken)
    {
        var candidates = await waitlistEntryRepository.GetWaitingCandidatesAsync(
            organizationId, locationId, serviceId, employeeId, freedSlot, cancellationToken);
        if (candidates.Count == 0)
            return;

        var offerWindow = await dbContext.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.WaitlistOfferWindow)
            .FirstAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;

        foreach (var candidate in candidates)
        {
            if (candidate.Offer(employeeId, freedSlot, utcNow + offerWindow, utcNow).IsFailure)
                continue;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }
    }
}