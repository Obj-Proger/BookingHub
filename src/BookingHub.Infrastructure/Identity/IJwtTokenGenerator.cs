namespace BookingHub.Infrastructure.Identity;

/// <summary>
/// Infrastructure-internal — Application never needs to know a JWT is involved at all, only
/// that <see cref="Common.Security.ICurrentUser"/> resolves to a UserId somehow.
/// </summary>
internal interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}