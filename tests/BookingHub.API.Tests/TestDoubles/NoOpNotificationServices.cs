using BookingHub.Infrastructure.Notifications;

namespace BookingHub.API.Tests.TestDoubles;

/// <summary>Real SmtpEmailService/TwilioSmsService need working SMTP/Twilio credentials this
/// test run has none of — replaced wholesale via ApiWebApplicationFactory, not configured
/// around, since there is nothing valid to configure them with here. The outbox processor
/// still runs, so queued messages are processed and then dropped here.</summary>
internal sealed class NoOpEmailService : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class NoOpSmsService : ISmsService
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}