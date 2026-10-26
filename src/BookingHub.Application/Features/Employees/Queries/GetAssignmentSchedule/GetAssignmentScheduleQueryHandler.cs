using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetAssignmentSchedule;

internal sealed class GetAssignmentScheduleQueryHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IQueryHandler<GetAssignmentScheduleQuery, AssignmentScheduleResponse>
{
    public async Task<Result<AssignmentScheduleResponse>> Handle(GetAssignmentScheduleQuery query, CancellationToken cancellationToken)
    {
        var assignment = await (
            from a in dbContext.EmployeeLocationAssignments
            join l in dbContext.Locations on a.LocationId equals l.Id
            join e in dbContext.Employees on a.EmployeeId equals e.Id
            where a.Id == query.AssignmentId
                && a.LocationId == query.LocationId
                && l.OrganizationId == query.OrganizationId
                && e.OrganizationId == query.OrganizationId
            select new { AssignmentId = a.Id, a.EmployeeId, EmployeeName = e.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
            return Result.Failure<AssignmentScheduleResponse>(ApplicationErrors.EmployeeLocationAssignment.NotFound);

        var recurringSchedules = await dbContext.RecurringSchedules
            .Where(s => s.EmployeeLocationAssignmentId == assignment.AssignmentId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new RecurringScheduleResponse(s.Id, s.DayOfWeek, s.StartTime, s.EndTime))
            .ToListAsync(cancellationToken);

        var earliestDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(-1);

        var exceptions = await dbContext.ScheduleExceptions
            .Where(x => x.EmployeeLocationAssignmentId == assignment.AssignmentId && x.Date >= earliestDate)
            .OrderBy(x => x.Date)
            .Select(x => new ScheduleExceptionResponse(x.Id, x.Date, x.Type, x.ModifiedStartTime, x.ModifiedEndTime))
            .ToListAsync(cancellationToken);

        return new AssignmentScheduleResponse(
            assignment.AssignmentId, assignment.EmployeeId, assignment.EmployeeName, recurringSchedules, exceptions);
    }
}