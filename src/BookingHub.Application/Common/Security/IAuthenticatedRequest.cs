namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request that needs a signed-in caller but no organization membership: the caller acts
/// on their own behalf (their memberships, creating a new organization).
/// </summary>
public interface IAuthenticatedRequest;