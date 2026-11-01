using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Common.Behaviors;

/// <summary>
/// Enforces the access marker every request declares (see <see cref="RequestAccessValidation"/>).
/// Fails closed: a request without a marker is a programming error and is never let through.
/// </summary>
internal sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser, IOrganizationMemberRepository organizationMemberRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Func<Error, TResponse> BuildFailure = FailureResponseFactory.Create<TResponse>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        switch (request)
        {
            case IPublicRequest or ISystemRequest:
                return await next();

            case IAuthenticatedRequest:
                // Reading the identity throws when nobody is signed in: an anonymous caller here means an
                // endpoint is missing [Authorize] — a configuration bug, not a business outcome.
                _ = currentUser.UserId;
                return await next();

            case IRequireOrganizationMembership organizationScoped:
                var failure = await AuthorizeMemberAsync(request, organizationScoped, cancellationToken);
                return failure is null ? await next() : BuildFailure(failure);

            default:
                throw new InvalidOperationException(
                    $"{typeof(TRequest).Name} declares no access marker; see {nameof(RequestAccessValidation)}.");
        }
    }

    /// <returns>The reason the caller may not send <paramref name="request"/>, or null when they may.</returns>
    private async Task<Error?> AuthorizeMemberAsync(
        TRequest request, IRequireOrganizationMembership organizationScoped, CancellationToken cancellationToken)
    {
        var member = await organizationMemberRepository.GetByOrganizationAndUserAsync(
            organizationScoped.OrganizationId, currentUser.UserId, cancellationToken);

        if (member is null)
            return ApplicationErrors.Authorization.NotAMember;

        var isOrgWideManager = member.Role is OrganizationRole.Owner or OrganizationRole.Administrator;

        var allowed = request switch
        {
            IRequireBookingAccess booking =>
                isOrgWideManager
                || (member.Role == OrganizationRole.LocationManager && member.LocationId == booking.LocationId)
                || (member.Role == OrganizationRole.Employee && member.EmployeeId == booking.EmployeeId),

            IRequireLocationManagement location =>
                isOrgWideManager
                || (member.Role == OrganizationRole.LocationManager && member.LocationId == location.LocationId),

            IRequireOrganizationManagement => isOrgWideManager,

            _ => true // plain membership, any role
        };

        return allowed ? null : ApplicationErrors.Authorization.InsufficientRole;
    }
}