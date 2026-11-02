using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.RefreshTokens;

/// <summary>Public by design: the access token has usually expired by the time a client refreshes it.</summary>
public sealed record RefreshTokensCommand(string RefreshToken) : ICommand<AuthenticatedResponse>, IPublicRequest;