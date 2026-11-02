using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.Register;

internal sealed class RegisterCommandHandler(IIdentityService identityService, IAuthTokenService authTokenService)
    : ICommandHandler<RegisterCommand, AuthenticatedResponse>
{
    public async Task<Result<AuthenticatedResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var userId = await identityService.RegisterAsync(command.Email, command.Password, cancellationToken);
        if (userId.IsFailure)
            return Result.Failure<AuthenticatedResponse>(userId.Error);

        var tokens = await authTokenService.IssueAsync(userId.Value, cancellationToken);
        return AuthenticatedResponse.From(tokens);
    }
}