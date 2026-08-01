using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Common.Behaviors;

internal sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser, IOrganizationMemberRepository organizationMemberRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Func<Error, TResponse> BuildFailure = FailureResponseFactory.Create<TResponse>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var organizationId = request switch
        {
            IRequireOrganizationMembership scoped => scoped.OrganizationId,
            _ => (Guid?)null
        };

        if (organizationId is null)
            return await next();

        var member = await organizationMemberRepository.GetByOrganizationAndUserAsync(
            organizationId.Value, currentUser.UserId, cancellationToken);

        if (member is null)
            return BuildFailure(ApplicationErrors.Authorization.NotAMember);

        if (request is IRequireOrganizationManagement && member.Role is not (OrganizationRole.Owner or OrganizationRole.Administrator))
            return BuildFailure(ApplicationErrors.Authorization.InsufficientRole);

        return await next();
    }
}