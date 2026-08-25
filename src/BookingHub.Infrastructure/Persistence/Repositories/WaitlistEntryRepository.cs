using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class WaitlistEntryRepository(ApplicationDbContext dbContext) : IWaitlistEntryRepository
{
    public void Add(WaitlistEntry entry) => dbContext.WaitlistEntries.Add(entry);

    public Task<WaitlistEntry?> GetByIdAsync(Guid waitlistEntryId, CancellationToken cancellationToken) =>
        dbContext.WaitlistEntries.FirstOrDefaultAsync(e => e.Id == waitlistEntryId, cancellationToken);

    public async Task<IReadOnlyList<WaitlistEntry>> GetWaitingCandidatesAsync(
        Guid organizationId, Guid locationId, Guid serviceId, Guid employeeId, TimeSlot freedSlot, CancellationToken cancellationToken) =>
        await dbContext.WaitlistEntries
            .Where(e => e.OrganizationId == organizationId && e.LocationId == locationId && e.ServiceId == serviceId
                && e.Status == WaitlistEntryStatus.Waiting
                && (e.EmployeeId == null || e.EmployeeId == employeeId)
                && e.DesiredWindow.StartUtc < freedSlot.EndUtc && freedSlot.StartUtc < e.DesiredWindow.EndUtc)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}