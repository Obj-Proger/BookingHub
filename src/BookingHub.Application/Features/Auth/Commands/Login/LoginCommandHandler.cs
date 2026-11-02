using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.Login;

internal sealed class LoginCommandHandler(IIdentityService identityService, IAuthTokenService authTokenService)
    : ICommandHandler<LoginCommand, AuthenticatedResponse>
{
    public async Task<Result<AuthenticatedResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // One error for an unknown email and a wrong password alike: which emails are registered is not disclosed.
        var userId = await identityService.VerifyCredentialsAsync(command.Email, command.Password, cancellationToken);
        if (userId is null)
            return Result.Failure<AuthenticatedResponse>(ApplicationErrors.Auth.InvalidCredentials);

        var tokens = await authTokenService.IssueAsync(userId.Value, cancellationToken);
        return AuthenticatedResponse.From(tokens);
    }
}