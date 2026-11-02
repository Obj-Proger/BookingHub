namespace BookingHub.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>How often the table is checked once it has been drained — short, so a guest gets their SMS within seconds.</summary>
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);
}
