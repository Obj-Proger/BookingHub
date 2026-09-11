namespace BookingHub.Application.Common.Security;

/// <summary>
/// The organization the current request is scoped to, resolved from the {organizationId}
/// route segment on authorized routes. Null on public routes (which use {organizationSlug}
/// instead and resolve OrganizationId explicitly within the handler) — unlike
/// <see cref="ICurrentUser"/>, a missing value here is an expected, valid state, not a bug.
/// </summary>
public interface ICurrentTenant
{
    Guid? OrganizationId { get; }
}