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

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatching domain event {DomainEventType}")]
    public static partial void DispatchingDomainEvent(ILogger logger, string domainEventType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Domain event dispatch failed for: {EventTypes}")]
    public static partial void DomainEventDispatchFailed(ILogger logger, Exception exception, string eventTypes);
}