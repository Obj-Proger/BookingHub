using System.Collections.Concurrent;
using BookingHub.Infrastructure.Notifications;

namespace BookingHub.API.Tests.TestDoubles;

/// <summary>
/// Every SMS and email the API sends, kept in memory in place of a real gateway (a test run has no SMTP
/// or Twilio credentials). Tests read guest links out of it the way a guest would.
/// </summary>
public sealed class GuestInbox
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(100);

    private readonly ConcurrentQueue<(string To, string Body)> _messages = new();

    public void Deliver(string to, string body) => _messages.Enqueue((to, body));

    /// <summary>
    /// The first message to <paramref name="to"/> that <paramref name="matches"/>, waiting for it if it has
    /// not arrived yet: messages go out through the outbox, a moment after the request that sent them.
    /// </summary>
    public async Task<string> WaitForAsync(string to, Func<string, bool> matches, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        while (true)
        {
            foreach (var (recipient, body) in _messages)
            {
                if (recipient == to && matches(body))
                    return body;
            }

            try
            {
                await Task.Delay(PollingInterval, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var received = string.Join(Environment.NewLine, _messages.Where(m => m.To == to).Select(m => m.Body));
                throw new TimeoutException(
                    $"The expected message to {to} did not arrive within {Timeout.TotalSeconds} s. It got:{Environment.NewLine}{received}");
            }
        }
    }

    /// <summary>The <paramref name="page"/> link (confirm, manage, review or waitlist) sent to <paramref name="to"/>.</summary>
    /// <param name="id">The booking or waitlist entry the link has to be for; any, when null.</param>
    public async Task<GuestLink> WaitForLinkAsync(string to, string page, Guid? id, CancellationToken cancellationToken)
    {
        var body = await WaitForAsync(
            to, text => GuestLink.Find(text, page) is { } link && (id is null || link.Id == id), cancellationToken);

        return GuestLink.Find(body, page)!;
    }
}

internal sealed class RecordingSmsService(GuestInbox inbox) : ISmsService
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        inbox.Deliver(message.ToPhoneNumber, message.Body);
        return Task.CompletedTask;
    }
}

internal sealed class RecordingEmailService(GuestInbox inbox) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        inbox.Deliver(message.ToAddress, message.Body);
        return Task.CompletedTask;
    }
}
