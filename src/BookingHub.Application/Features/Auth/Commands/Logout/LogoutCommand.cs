using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Auth.Commands.Logout;

/// <summary>Public by design, like refreshing: the refresh token itself proves the session.</summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand, IPublicRequest;