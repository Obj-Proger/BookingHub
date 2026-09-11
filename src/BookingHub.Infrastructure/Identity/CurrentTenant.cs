using BookingHub.Application.Common.Security;

namespace BookingHub.Infrastructure.Identity;

internal sealed class CurrentTenant(TenantContext tenantContext) : ICurrentTenant
{
    public Guid? OrganizationId => tenantContext.OrganizationId;
}