using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Auth;

/// <summary>Sessions live in process memory, so an application restart signs everyone out.</summary>
internal sealed class InMemoryTokenStore(TimeProvider clock) : ITokenStore
{
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromDays(14);

    private readonly ConcurrentDictionary<string, TokenSession> _sessions = new();

    public string Create(AuthenticatedResponse tokens)
    {
        var now = clock.GetUtcNow();
        RemoveIdleSessions(now);

        var sessionId = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        _sessions[sessionId] = new TokenSession(tokens, now);
        return sessionId;
    }

    public bool TryGet(string sessionId, [NotNullWhen(true)] out TokenSession? session)
    {
        var now = clock.GetUtcNow();

        if (_sessions.TryGetValue(sessionId, out var found))
        {
            if (!IsIdle(found, now))
            {
                found.Touch(now);
                session = found;
                return true;
            }

            _sessions.TryRemove(sessionId, out _);
        }

        session = null;
        return false;
    }

    public void Update(string sessionId, AuthenticatedResponse tokens)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
            session.Tokens = tokens;
    }

    public void Remove(string sessionId) => _sessions.TryRemove(sessionId, out _);

    private void RemoveIdleSessions(DateTimeOffset now)
    {
        foreach (var (sessionId, session) in _sessions)
        {
            if (IsIdle(session, now))
                _sessions.TryRemove(sessionId, out _);
        }
    }

    private static bool IsIdle(TokenSession session, DateTimeOffset now) =>
        now - session.LastUsedAtUtc > IdleTimeout;
}