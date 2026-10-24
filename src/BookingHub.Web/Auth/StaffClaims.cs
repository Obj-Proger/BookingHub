using System.Security.Claims;

namespace BookingHub.Web.Auth;

internal static class StaffClaimTypes
{
    public const string SessionId = "sid";
}

internal static class StaffClaimsPrincipalExtensions
{
    public static string? GetSessionId(this ClaimsPrincipal principal) =>
        principal.FindFirst(StaffClaimTypes.SessionId)?.Value;
}