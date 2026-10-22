using BookingHub.Application.Common.Notifications;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.Notifications;

/// <summary>Writes each email to the log instead of delivering it.</summary>
internal sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email to {ToAddress}: {Subject} — {Body}", message.ToAddress, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}