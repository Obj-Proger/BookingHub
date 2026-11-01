using BookingHub.Infrastructure.Logging;
using BookingHub.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Deletes processed outbox messages once they are a week old — long enough to look into a recent
/// delivery. Abandoned messages are kept until someone has looked at them.
/// </summary>
internal sealed class OutboxCleanupJob(ApplicationDbContext dbContext, TimeProvider timeProvider, ILogger<OutboxCleanupJob> logger)
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task PurgeProcessedMessagesAsync(IJobCancellationToken cancellationToken)
    {
        var cutoffUtc = timeProvider.GetUtcNow().UtcDateTime - Retention;

        var count = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken.ShutdownToken);

        Log.PurgedOutboxMessages(logger, count);
    }
}