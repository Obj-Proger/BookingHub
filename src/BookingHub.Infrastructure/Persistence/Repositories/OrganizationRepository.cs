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

    public Task<string?> GetSlugAsync(Guid organizationId, CancellationToken cancellationToken) =>
        dbContext.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.Slug)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task DeleteAllDataAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        // Every foreign key in this schema is DeleteBehavior.Restrict, so nothing cascades on its own:
        // rows are deleted leaves first, and the Organization itself last (Remove, by the caller).
        // Client rows are never touched — clients are global, shared across organizations.
        //
        // Every statement is filtered by this organization explicitly. Row-level security and the
        // query filters scope these deletes as well, but must never be the only thing that does.
        var locationIds = dbContext.Locations
            .Where(l => l.OrganizationId == organizationId)
            .Select(l => l.Id);
        var assignmentIds = dbContext.EmployeeLocationAssignments
            .Where(a => locationIds.Contains(a.LocationId))
            .Select(a => a.Id);

        await dbContext.RecurringSchedules
            .Where(s => assignmentIds.Contains(s.EmployeeLocationAssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.ScheduleExceptions
            .Where(e => assignmentIds.Contains(e.EmployeeLocationAssignmentId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.LocationServiceOverrides
            .Where(o => locationIds.Contains(o.LocationId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.EmployeeLocationAssignments
            .Where(a => locationIds.Contains(a.LocationId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Reviews.Where(r => r.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.WaitlistEntries.Where(w => w.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Bookings.Where(b => b.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);

        // Memberships reference employees and locations (the scope of Employee / LocationManager roles).
        await dbContext.OrganizationMembers.Where(m => m.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);

        await dbContext.Employees.Where(e => e.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Services.Where(s => s.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Locations.Where(l => l.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);
    }
}