using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IRecurringScheduleRepository
{
    void Add(RecurringSchedule schedule);
    void Remove(RecurringSchedule schedule);

    /// <summary>All entries for one assignment on one day of week — the visibility a single
    /// RecurringSchedule instance in the domain doesn't have, needed to check non-overlap.</summary>
    Task<IReadOnlyList<RecurringSchedule>> GetByAssignmentAndDayAsync(
        Guid employeeLocationAssignmentId, DayOfWeek dayOfWeek, CancellationToken cancellationToken);

    /// <param name="locationId">Filtered via the owning EmployeeLocationAssignment's LocationId — same rule as every other location-scoped lookup in this project.</param>
    Task<RecurringSchedule?> GetByIdAsync(Guid locationId, Guid recurringScheduleId, CancellationToken cancellationToken);
}