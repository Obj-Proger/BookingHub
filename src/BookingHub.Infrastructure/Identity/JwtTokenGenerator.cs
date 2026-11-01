using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BookingHub.Infrastructure.Identity;

internal sealed class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : IJwtTokenGenerator
{
    private static readonly JsonWebTokenHandler Handler = new();
    private readonly JwtOptions _options = jwtOptions.Value;

    public string GenerateToken(ApplicationUser user)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            ]),
            // Set explicitly so all three timestamps come from the same clock.
            IssuedAt = utcNow,
            NotBefore = utcNow,
            Expires = utcNow.AddMinutes(_options.ExpiryMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = signingCredentials
        };

        return Handler.CreateToken(descriptor);
    }
}