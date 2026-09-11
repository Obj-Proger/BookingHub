namespace BookingHub.Application.Common.Security;

/// <summary>
/// Mutable, per-request holder for the resolved organization — populated once by
/// TenantResolutionMiddleware (API) after resolving either the {organizationId} or
/// {organizationSlug} route value against the database, then read by CurrentTenant
/// (Infrastructure). Exists because resolving a slug requires an async database call, which
/// ICurrentTenant's synchronous property getter cannot perform itself.
/// </summary>
public sealed class TenantContext
{
    public Guid? OrganizationId { get; set; }
}