using BookingHub.Application.Common.Notifications;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.Notifications;

/// <summary>Writes each SMS to the log instead of delivering it.</summary>
internal sealed class LoggingSmsService(ILogger<LoggingSmsService> logger) : ISmsService
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("SMS to {ToPhoneNumber}: {Body}", message.ToPhoneNumber, message.Body);
        return Task.CompletedTask;
    }
}