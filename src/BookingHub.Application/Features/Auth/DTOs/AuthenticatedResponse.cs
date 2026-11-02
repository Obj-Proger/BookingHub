using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Auth.DTOs;

public sealed record AuthenticatedResponse(string AccessToken, string RefreshToken)
{
    internal static AuthenticatedResponse From(AuthTokens tokens) => new(tokens.AccessToken, tokens.RefreshToken);
}