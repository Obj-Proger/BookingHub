namespace BookingHub.Infrastructure.Outbox;

/// <summary>
/// Deferred work stored in the same transaction as the change that caused it — a domain event to
/// hand to its handlers, or a message to deliver to a guest. <see cref="OutboxProcessor"/> picks it
/// up once that transaction has committed, so the work happens if and only if the change did.
/// </summary>
internal sealed class OutboxMessage
{
    public const int MaxTypeLength = 200;
    public const int MaxErrorLength = 4000;

    public Guid Id { get; private set; }

    /// <summary>The tenant scope the message is processed in.</summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>The payload's type name, e.g. BookingCancelledEvent or SmsMessage — see <see cref="OutboxSerializer"/>.</summary>
    public string Type { get; private set; } = null!;

    /// <summary>The payload as JSON.</summary>
    public string Payload { get; private set; } = null!;

    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }

    /// <summary>Failed attempts so far; the processor gives up at <see cref="OutboxProcessor.MaxAttempts"/>.</summary>
    public int Attempts { get; private set; }

    /// <summary>The message is not picked up before this time — the retry backoff after a failure.</summary>
    public DateTime NextAttemptOnUtc { get; private set; }

    /// <summary>The exception of the last failed attempt, kept for whoever investigates an abandoned message.</summary>
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(Guid organizationId, object payload, DateTime occurredOnUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        OrganizationId = organizationId,
        Type = OutboxSerializer.GetTypeName(payload),
        Payload = OutboxSerializer.Serialize(payload),
        OccurredOnUtc = occurredOnUtc,
        NextAttemptOnUtc = occurredOnUtc
    };

    public void MarkProcessed(DateTime utcNow) => ProcessedOnUtc = utcNow;
}