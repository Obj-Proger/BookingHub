using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Common.Persistence;

public interface IWaitlistEntryRepository
{
    void Add(WaitlistEntry entry);

    Task<WaitlistEntry?> GetByIdAsync(Guid waitlistEntryId, CancellationToken cancellationToken);

    /// <summary>
    /// Waiting entries matching the freed slot — same location/service, no specific employee
    /// requested or the same one who freed up, and a desired window overlapping the freed slot —
    /// ordered earliest-created first (FIFO, per the Vision Document's "first in queue" rule).
    /// </summary>
    Task<IReadOnlyList<WaitlistEntry>> GetWaitingCandidatesAsync(
        Guid organizationId, Guid locationId, Guid serviceId, Guid employeeId, TimeSlot freedSlot,
        CancellationToken cancellationToken);
}