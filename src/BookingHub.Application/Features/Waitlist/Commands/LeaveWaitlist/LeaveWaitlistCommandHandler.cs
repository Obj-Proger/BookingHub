using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Waitlist.Commands.LeaveWaitlist;

internal sealed class LeaveWaitlistCommandHandler(IWaitlistEntryRepository waitlistEntryRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<LeaveWaitlistCommand>
{
    public async Task<Result> Handle(LeaveWaitlistCommand command, CancellationToken cancellationToken)
    {
        var entry = await waitlistEntryRepository.GetByIdAsync(command.WaitlistEntryId, cancellationToken);
        if (entry is null)
            return Result.Failure(ApplicationErrors.WaitlistEntry.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!entry.ManagementToken.Matches(providedToken))
            return Result.Failure(ApplicationErrors.WaitlistEntry.InvalidManagementToken);

        var cancelResult = entry.Cancel(DateTime.UtcNow);
        if (cancelResult.IsFailure)
            return cancelResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}