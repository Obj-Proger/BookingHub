namespace BookingHub.Application.Common.Security;

/// <summary>
/// The authenticated caller of the current request. Populated by Infrastructure from the
/// active HTTP context/JWT — never trust an identity supplied as command or query data,
/// since that could be spoofed by the client.
/// </summary>
public interface ICurrentUser
{
    Guid UserId { get; }
}