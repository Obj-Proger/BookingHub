using System.Security.Claims;
using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingHub.Infrastructure.Identity;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            // Requires JwtBearerOptions.MapInboundClaims = false (see DependencyInjection) —
            // otherwise the validation pipeline silently renames "sub" to a legacy long claim
            // URI before this code ever runs, and FindFirstValue below would always return null.
            var subjectClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (subjectClaim is null || !Guid.TryParse(subjectClaim, out var userId))
            {
                throw new InvalidOperationException(
                    "No authenticated user on the current request — this indicates a route that should " +
                    "require authentication was reached without it, a configuration bug, not a business error.");
            }

            return userId;
        }
    }
}