namespace BookingHub.Infrastructure.Identity;

/// <summary>Application never needs to know a JWT is involved at all, only that ICurrentUser
/// resolves to a UserId somehow — but the API layer needs this directly, to issue tokens
/// after login/registration, which is genuinely outside Application's CQRS pipeline.</summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}