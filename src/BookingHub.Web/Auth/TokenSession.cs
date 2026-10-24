using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Auth;

/// <summary>The API tokens of one signed-in browser session. They never leave the server.</summary>
internal sealed class TokenSession(AuthenticatedResponse tokens, DateTimeOffset now)
{
    private volatile AuthenticatedResponse _tokens = tokens;
    private long _lastUsedTicks = now.UtcTicks;

    public AuthenticatedResponse Tokens
    {
        get => _tokens;
        set => _tokens = value;
    }

    /// <summary>Serializes refreshes of this session so a single-use refresh token is never replayed.</summary>
    public SemaphoreSlim RefreshLock { get; } = new(1, 1);

    public DateTimeOffset LastUsedAtUtc => new(Interlocked.Read(ref _lastUsedTicks), TimeSpan.Zero);

    public void Touch(DateTimeOffset now) => Interlocked.Exchange(ref _lastUsedTicks, now.UtcTicks);
}