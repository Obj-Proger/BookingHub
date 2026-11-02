using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthenticatedResponse>, IPublicRequest;