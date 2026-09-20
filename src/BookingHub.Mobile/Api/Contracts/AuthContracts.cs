namespace BookingHub.Mobile.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record AuthenticatedResponse(string AccessToken, string RefreshToken);