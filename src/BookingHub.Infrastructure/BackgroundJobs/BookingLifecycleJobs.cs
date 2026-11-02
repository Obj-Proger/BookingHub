using BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;
using BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;
using BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;
using BookingHub.Application.Features.Waitlist.Commands.ExpireWaitlistOffers;
using BookingHub.Infrastructure.Logging;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Thin adapters from Hangfire's method-call scheduling model onto the lifecycle commands
/// (Application) — each runs one command for every organization and logs how many records it
/// touched, for visibility in the Hangfire dashboard/logs.
/// </summary>
internal sealed class BookingLifecycleJobs(TenantJobRunner runner, ILogger<BookingLifecycleJobs> logger)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ExpirePendingBookingsAsync(IJobCancellationToken cancellationToken)
    {
        var count = await runner.RunForEachOrganizationAsync(
            organizationId => new ExpirePendingBookingsCommand(organizationId), cancellationToken.ShutdownToken);
        Log.ExpiredPendingBookings(logger, count);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task TransitionBookingsToAwaitingReviewAsync(IJobCancellationToken cancellationToken)
    {
        var count = await runner.RunForEachOrganizationAsync(
            organizationId => new TransitionBookingsToAwaitingReviewCommand(organizationId), cancellationToken.ShutdownToken);
        Log.TransitionedBookingsToAwaitingReview(logger, count);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task AutoCompleteBookingsAsync(IJobCancellationToken cancellationToken)
    {
        var count = await runner.RunForEachOrganizationAsync(
            organizationId => new AutoCompleteBookingsCommand(organizationId), cancellationToken.ShutdownToken);
        Log.AutoCompletedBookings(logger, count);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ExpireWaitlistOffersAsync(IJobCancellationToken cancellationToken)
    {
        var count = await runner.RunForEachOrganizationAsync(
            organizationId => new ExpireWaitlistOffersCommand(organizationId), cancellationToken.ShutdownToken);
        Log.ExpiredWaitlistOffers(logger, count);
    }
}