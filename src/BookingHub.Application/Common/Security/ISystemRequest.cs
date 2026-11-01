namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request the system sends on its own, from a background job — never on behalf of a
/// caller, so it must not be reachable through any API endpoint.
/// </summary>
public interface ISystemRequest;