using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Waitlist.Commands.ExpireWaitlistOffers;

internal sealed class ExpireWaitlistOffersCommandHandler(
    IWaitlistEntryRepository waitlistEntryRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<ExpireWaitlistOffersCommand, int>
{
    public async Task<Result<int>> Handle(ExpireWaitlistOffersCommand command, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var entries = await waitlistEntryRepository.GetOffersPastExpiryAsync(command.OrganizationId, utcNow, cancellationToken);

        var expiredCount = 0;
        foreach (var entry in entries)
        {
            if (entry.Expire(utcNow).IsSuccess)
                expiredCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return expiredCount;
    }
}