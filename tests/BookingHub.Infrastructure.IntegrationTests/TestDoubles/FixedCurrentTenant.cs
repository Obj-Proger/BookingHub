using BookingHub.Application.Common.Security;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

internal sealed class FixedCurrentTenant(Guid? organizationId) : ICurrentTenant
{
    public Guid? OrganizationId => organizationId;
}