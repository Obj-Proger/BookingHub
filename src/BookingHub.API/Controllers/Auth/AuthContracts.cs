using System.ComponentModel.DataAnnotations;

namespace BookingHub.API.Controllers.Auth;

public sealed record RegisterRequest([Required, EmailAddress] string Email, [Required, MinLength(8)] string Password);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record RefreshRequest([Required] string RefreshToken);
public sealed record LogoutRequest([Required] string RefreshToken);
public sealed record AuthenticatedResponse(string AccessToken, string RefreshToken);