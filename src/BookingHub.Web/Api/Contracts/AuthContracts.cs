namespace BookingHub.Web.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthenticatedResponse(string AccessToken, string RefreshToken);