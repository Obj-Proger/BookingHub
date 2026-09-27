using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationRepository(ApplicationDbContext dbContext) : IOrganizationRepository
{
    public void Add(Organization organization) => dbContext.Organizations.Add(organization);

    public void Remove(Organization organization) => dbContext.Organizations.Remove(organization);

    public Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken) =>
        dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken);

    public async Task DeleteAllDataAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        // Deletion order follows the FK graph (every FK in this schema is DeleteBehavior.Restrict,
        // so nothing cascades on its own): leaves first, Organization itself last (Remove, separately).
        // Client rows are never touched — clients are global, shared across organizations.

        // These four tables have no OrganizationId column at all (they only carry EmployeeId/
        // LocationId — this codebase has no navigation properties to join through in LINQ), so
        // there is nothing to filter by here. Scoping is enforced by Postgres RLS instead: each of
        // these tables has a join-based "tenant_isolation" policy keyed off app.current_organization_id
        // (set by TenantResolutionMiddleware for the whole request) — see the InitialCreate migration.
        await dbContext.RecurringSchedules.ExecuteDeleteAsync(cancellationToken);
        await dbContext.ScheduleExceptions.ExecuteDeleteAsync(cancellationToken);
        await dbContext.LocationServiceOverrides.ExecuteDeleteAsync(cancellationToken);
        await dbContext.EmployeeLocationAssignments.ExecuteDeleteAsync(cancellationToken);

        await dbContext.Reviews.Where(r => r.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.WaitlistEntries.Where(w => w.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Bookings.Where(b => b.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Employees.Where(e => e.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Services.Where(s => s.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Locations.Where(l => l.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.OrganizationMembers.Where(m => m.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
    }
}