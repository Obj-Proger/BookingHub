using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.Register;

/// <summary>Creates an account and signs it in. The email and password rules are the Identity store's own.</summary>
public sealed record RegisterCommand(string Email, string Password) : ICommand<AuthenticatedResponse>, IPublicRequest;