using System.ComponentModel.DataAnnotations;

namespace BookingHub.API.Controllers.Auth;

public sealed record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public sealed record AuthenticatedResponse(string AccessToken);