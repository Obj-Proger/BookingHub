using BookingHub.Application.Common.Notifications;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

internal sealed class NoOpSmsService : ISmsService
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}