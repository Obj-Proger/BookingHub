using Hangfire;

namespace BookingHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Declares the recurring schedule once. Called from API's startup (Program.cs, not yet
/// written) — not from AddInfrastructure itself, because scheduling recurring jobs requires
/// a built IServiceProvider (to resolve JobStorage.Current's configuration), which an
/// IServiceCollection extension method does not have access to yet.
/// </summary>
public static class HangfireJobScheduler
{
    public static void ScheduleRecurringJobs()
    {
        RecurringJob.AddOrUpdate<BookingLifecycleJobs>(
            "expire-pending-bookings",
            job => job.ExpirePendingBookingsAsync(JobCancellationToken.Null),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<BookingLifecycleJobs>(
            "transition-bookings-to-awaiting-review",
            job => job.TransitionBookingsToAwaitingReviewAsync(JobCancellationToken.Null),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<BookingLifecycleJobs>(
            "auto-complete-bookings",
            job => job.AutoCompleteBookingsAsync(JobCancellationToken.Null),
            Cron.Hourly());
    }
}