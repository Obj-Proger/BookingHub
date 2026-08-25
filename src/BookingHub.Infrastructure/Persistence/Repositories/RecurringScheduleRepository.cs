using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class RecurringScheduleRepository(ApplicationDbContext dbContext) : IRecurringScheduleRepository
{
    public void Add(RecurringSchedule schedule) => dbContext.RecurringSchedules.Add(schedule);
    public void Remove(RecurringSchedule schedule) => dbContext.RecurringSchedules.Remove(schedule);

    public Task<IReadOnlyList<RecurringSchedule>> GetByAssignmentAndDayAsync(
        Guid employeeLocationAssignmentId, DayOfWeek dayOfWeek, CancellationToken cancellationToken) =>
        dbContext.RecurringSchedules
            .Where(s => s.EmployeeLocationAssignmentId == employeeLocationAssignmentId && s.DayOfWeek == dayOfWeek)
            .Cast<RecurringSchedule>()
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<RecurringSchedule>)t.Result, cancellationToken);

    public Task<RecurringSchedule?> GetByIdAsync(Guid locationId, Guid recurringScheduleId, CancellationToken cancellationToken) =>
        dbContext.RecurringSchedules
            .Join(dbContext.EmployeeLocationAssignments, s => s.EmployeeLocationAssignmentId, a => a.Id, (s, a) => new { Schedule = s, a.LocationId })
            .Where(x => x.Schedule.Id == recurringScheduleId && x.LocationId == locationId)
            .Select(x => x.Schedule)
            .FirstOrDefaultAsync(cancellationToken);
}