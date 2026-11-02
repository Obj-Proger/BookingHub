using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Auth.Commands.Logout;

internal sealed class LogoutCommandHandler(IAuthTokenService authTokenService) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await authTokenService.RevokeAsync(command.RefreshToken, cancellationToken);
        return Result.Success();
    }
}