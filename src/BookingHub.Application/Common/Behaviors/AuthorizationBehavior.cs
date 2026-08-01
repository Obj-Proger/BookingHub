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
        if (request is not IRequireOrganizationMembership scoped)
            return await next();

        var member = await organizationMemberRepository.GetByOrganizationAndUserAsync(
            scoped.OrganizationId, currentUser.UserId, cancellationToken);

        if (member is null)
            return BuildFailure(ApplicationErrors.Authorization.NotAMember);

        var isOrgWideManager = member.Role is OrganizationRole.Owner or OrganizationRole.Administrator;

        if (request is IRequireLocationManagement locationScoped)
        {
            var isScopedToThisLocation =
                member.Role == OrganizationRole.LocationManager && member.LocationId == locationScoped.LocationId;

            if (!isOrgWideManager && !isScopedToThisLocation)
                return BuildFailure(ApplicationErrors.Authorization.InsufficientRole);
        }
        else if (request is IRequireOrganizationManagement && !isOrgWideManager)
        {
            return BuildFailure(ApplicationErrors.Authorization.InsufficientRole);
        }

        return await next();
    }
}