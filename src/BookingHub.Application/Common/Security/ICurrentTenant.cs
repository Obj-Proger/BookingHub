namespace BookingHub.Application.Common.Security;

/// <summary>
/// The organization the current unit of work is scoped to (see <see cref="ITenantScope"/>). Null
/// when nothing names an organization — signing in, the caller's own memberships — which, unlike a
/// missing <see cref="ICurrentUser"/>, is an expected state, not a bug.
/// </summary>
public interface ICurrentTenant
{
    Guid? OrganizationId { get; }
}