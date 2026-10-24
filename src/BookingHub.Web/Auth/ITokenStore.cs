using System.Diagnostics.CodeAnalysis;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Auth;

internal interface ITokenStore
{
    /// <summary>Stores a new session and returns its id.</summary>
    string Create(AuthenticatedResponse tokens);

    /// <summary>Finds a live session and marks it as used; idle sessions are treated as missing.</summary>
    bool TryGet(string sessionId, [NotNullWhen(true)] out TokenSession? session);

    void Update(string sessionId, AuthenticatedResponse tokens);

    void Remove(string sessionId);
}