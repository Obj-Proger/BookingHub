using BookingHub.Application.Common.Notifications;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

internal sealed class NoOpEmailService : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}