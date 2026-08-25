using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class ScheduleExceptionRepository(ApplicationDbContext dbContext) : IScheduleExceptionRepository
{
    public void Add(ScheduleException exception) => dbContext.ScheduleExceptions.Add(exception);
    public void Remove(ScheduleException exception) => dbContext.ScheduleExceptions.Remove(exception);

    public Task<bool> ExistsForDateAsync(Guid employeeLocationAssignmentId, DateOnly date, CancellationToken cancellationToken) =>
        dbContext.ScheduleExceptions.AnyAsync(e => e.EmployeeLocationAssignmentId == employeeLocationAssignmentId && e.Date == date, cancellationToken);

    public Task<ScheduleException?> GetByIdAsync(Guid locationId, Guid scheduleExceptionId, CancellationToken cancellationToken) =>
        dbContext.ScheduleExceptions
            .Join(dbContext.EmployeeLocationAssignments, e => e.EmployeeLocationAssignmentId, a => a.Id, (e, a) => new { Exception = e, a.LocationId })
            .Where(x => x.Exception.Id == scheduleExceptionId && x.LocationId == locationId)
            .Select(x => x.Exception)
            .FirstOrDefaultAsync(cancellationToken);
}