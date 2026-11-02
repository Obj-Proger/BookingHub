using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Auth.DTOs;

namespace BookingHub.Application.Features.Auth.Commands.RefreshTokens;

internal sealed class RefreshTokensCommandHandler(IAuthTokenService authTokenService)
    : ICommandHandler<RefreshTokensCommand, AuthenticatedResponse>
{
    public async Task<Result<AuthenticatedResponse>> Handle(RefreshTokensCommand command, CancellationToken cancellationToken)
    {
        var tokens = await authTokenService.RefreshAsync(command.RefreshToken, cancellationToken);
        return tokens is null
            ? Result.Failure<AuthenticatedResponse>(ApplicationErrors.Auth.InvalidRefreshToken)
            : AuthenticatedResponse.From(tokens);
    }
}