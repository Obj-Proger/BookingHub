using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.Logging;

/// <summary>
/// Source-generated log methods — the compiler generates an internal IsEnabled(...) check
/// inside each partial method before its arguments are formatted, which neither a plain
/// ILogger.LogInformation(...) call nor a hand-extracted local variable can provide (CA1873).
/// </summary>
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Expired {Count} pending bookings past their confirmation window")]
    public static partial void ExpiredPendingBookings(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transitioned {Count} bookings to AwaitingReview")]
    public static partial void TransitionedBookingsToAwaitingReview(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Auto-completed {Count} bookings past their auto-complete window")]
    public static partial void AutoCompletedBookings(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Expired {Count} waitlist offers past their confirmation window")]
    public static partial void ExpiredWaitlistOffers(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "{CommandType} failed for organization {OrganizationId}")]
    public static partial void BackgroundCommandFailed(ILogger logger, Exception exception, string commandType, Guid organizationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {MessageId} ({MessageType}) failed on attempt {Attempts}; it will be retried")]
    public static partial void OutboxMessageFailed(ILogger logger, Exception exception, string messageType, Guid messageId, int attempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox message {MessageId} ({MessageType}) failed {Attempts} times and was abandoned")]
    public static partial void OutboxMessageAbandoned(ILogger logger, Exception exception, string messageType, Guid messageId, int attempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "Polling the outbox failed")]
    public static partial void OutboxPollingFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {Count} processed outbox messages")]
    public static partial void PurgedOutboxMessages(ILogger logger, int count);
}