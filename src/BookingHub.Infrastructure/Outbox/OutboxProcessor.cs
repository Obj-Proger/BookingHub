using BookingHub.Application.Common.Messaging;
using BookingHub.Infrastructure.Logging;
using BookingHub.Infrastructure.Notifications;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingHub.Infrastructure.Outbox;

/// <summary>
/// Works through the outbox: each message in its own DI scope and transaction, inside its
/// organization's tenant scope, so event handlers run under row-level security exactly like a
/// request. A row lock with SKIP LOCKED lets several API instances share the table without
/// processing a message twice. A failed message is retried with a growing delay and abandoned
/// after <see cref="MaxAttempts"/>; delivery is at-least-once, so handlers must tolerate a repeat.
/// </summary>
internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory, TimeProvider timeProvider, IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger)
    : BackgroundService
{
    public const int MaxAttempts = 10;

    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

    private const string DispatchSavepoint = "outbox_dispatch";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollingInterval, timeProvider);

        do
        {
            try
            {
                await DrainAsync(stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                // E.g. the database is unreachable: try again on the next tick instead of stopping the service.
                Log.OutboxPollingFailed(logger, exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        var processed = true;
        while (processed)
            processed = await TryProcessNextAsync(cancellationToken);
    }

    /// <returns>False when no message was due.</returns>
    private async Task<bool> TryProcessNextAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var message = await ClaimNextAsync(dbContext, cancellationToken);
        if (message is null)
            return false;

        // The savepoint keeps the row lock when the work fails: everything the handler did is undone,
        // but the failure is still recorded on the claimed row in the same transaction.
        await transaction.CreateSavepointAsync(DispatchSavepoint, cancellationToken);

        try
        {
            await services.GetRequiredService<RowLevelSecuritySession>().EnterAsync(message.OrganizationId, cancellationToken);
            await DispatchAsync(services, message, cancellationToken);

            message.MarkProcessed(timeProvider.GetUtcNow().UtcDateTime);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            await transaction.RollbackToSavepointAsync(DispatchSavepoint, CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            await RecordFailureAsync(dbContext, message, exception, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<OutboxMessage?> ClaimNextAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Not composed with any LINQ operator: EF Core runs the SQL as written, FOR UPDATE included.
        var claimed = await dbContext.OutboxMessages
            .FromSql($"""
                SELECT * FROM "OutboxMessages"
                WHERE "ProcessedOnUtc" IS NULL AND "Attempts" < {MaxAttempts} AND "NextAttemptOnUtc" <= {utcNow}
                ORDER BY "NextAttemptOnUtc"
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return claimed.SingleOrDefault();
    }

    private static async Task DispatchAsync(IServiceProvider services, OutboxMessage message, CancellationToken cancellationToken)
    {
        switch (OutboxSerializer.Deserialize(message.Type, message.Payload))
        {
            case IDomainEvent domainEvent:
                await services.GetRequiredService<IDomainEventDispatcher>().DispatchAsync([domainEvent], cancellationToken);
                break;

            case EmailMessage email:
                await services.GetRequiredService<IEmailService>().SendAsync(email, cancellationToken);
                break;

            case SmsMessage sms:
                await services.GetRequiredService<ISmsService>().SendAsync(sms, cancellationToken);
                break;

            default:
                throw new InvalidOperationException($"No processing is defined for outbox message type '{message.Type}'.");
        }
    }

    private async Task RecordFailureAsync(
        ApplicationDbContext dbContext, OutboxMessage message, Exception exception, CancellationToken cancellationToken)
    {
        var attempts = message.Attempts + 1;
        var nextAttemptOnUtc = timeProvider.GetUtcNow().UtcDateTime + GetRetryDelay(attempts);
        var error = exception.ToString();
        if (error.Length > OutboxMessage.MaxErrorLength)
            error = error[..OutboxMessage.MaxErrorLength];

        await dbContext.OutboxMessages
            .Where(m => m.Id == message.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Attempts, attempts)
                .SetProperty(m => m.NextAttemptOnUtc, nextAttemptOnUtc)
                .SetProperty(m => m.LastError, error),
                cancellationToken);

        if (attempts >= MaxAttempts)
            Log.OutboxMessageAbandoned(logger, exception, message.Type, message.Id, attempts);
        else
            Log.OutboxMessageFailed(logger, exception, message.Type, message.Id, attempts);
    }

    /// <summary>30 seconds after the first failure, doubling each time, at most an hour.</summary>
    private static TimeSpan GetRetryDelay(int attempts)
    {
        var delay = FirstRetryDelay * Math.Pow(2, attempts - 1);
        return delay < MaxRetryDelay ? delay : MaxRetryDelay;
    }
}