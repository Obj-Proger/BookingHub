using BookingHub.Application.Common.Security;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// The organization the current scope works in, as set through <see cref="RowLevelSecuritySession"/>.
/// Read by the query filters of <see cref="ApplicationDbContext"/> via <see cref="ICurrentTenant"/>.
/// </summary>
public sealed class TenantContext : ICurrentTenant
{
    public Guid? OrganizationId { get; set; }
}