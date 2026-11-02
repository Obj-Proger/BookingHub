using BookingHub.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.Notifications;

/// <summary>Writes each email to the log instead of delivering it.</summary>
internal sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Log.EmailNotSent(logger, message.ToAddress, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}