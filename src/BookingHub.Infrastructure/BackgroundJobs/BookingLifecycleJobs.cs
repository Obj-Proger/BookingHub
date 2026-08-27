using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;
using BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;
using BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Thin adapters from Hangfire's method-call scheduling model onto the three
/// background lifecycle commands (Application) — each just dispatches one command
/// and logs how many bookings it touched, for visibility in the Hangfire dashboard/logs.
/// </summary>
internal sealed class BookingLifecycleJobs(IDispatcher dispatcher, ILogger<BookingLifecycleJobs> logger)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ExpirePendingBookingsAsync(IJobCancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new ExpirePendingBookingsCommand(), cancellationToken.ShutdownToken);
        logger.LogInformation("Expired {Count} pending bookings past their confirmation window", result.Value);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task TransitionBookingsToAwaitingReviewAsync(IJobCancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new TransitionBookingsToAwaitingReviewCommand(), cancellationToken.ShutdownToken);
        logger.LogInformation("Transitioned {Count} bookings to AwaitingReview", result.Value);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task AutoCompleteBookingsAsync(IJobCancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new AutoCompleteBookingsCommand(), cancellationToken.ShutdownToken);
        logger.LogInformation("Auto-completed {Count} bookings past their auto-complete window", result.Value);
    }
}