using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IScheduleExceptionRepository
{
    void Add(ScheduleException exception);
    void Remove(ScheduleException exception);
    Task<bool> ExistsForDateAsync(Guid employeeLocationAssignmentId, DateOnly date, CancellationToken cancellationToken);

    /// <param name="locationId">Filtered via the owning EmployeeLocationAssignment's LocationId.</param>
    Task<ScheduleException?> GetByIdAsync(Guid locationId, Guid scheduleExceptionId, CancellationToken cancellationToken);
}