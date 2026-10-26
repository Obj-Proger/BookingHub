using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployeeAssignments;

internal sealed class GetEmployeeAssignmentsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetEmployeeAssignmentsQuery, IReadOnlyList<EmployeeAssignmentResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeAssignmentResponse>>> Handle(
        GetEmployeeAssignmentsQuery query, CancellationToken cancellationToken)
    {
        var employeeExists = await dbContext.Employees
            .AnyAsync(e => e.Id == query.EmployeeId && e.OrganizationId == query.OrganizationId, cancellationToken);

        if (!employeeExists)
            return Result.Failure<IReadOnlyList<EmployeeAssignmentResponse>>(ApplicationErrors.Employee.NotFound);

        var assignments = await (
            from a in dbContext.EmployeeLocationAssignments
            join l in dbContext.Locations on a.LocationId equals l.Id
            where a.EmployeeId == query.EmployeeId && a.IsActive && l.OrganizationId == query.OrganizationId
            orderby l.Name
            select new EmployeeAssignmentResponse(a.Id, l.Id, l.Name))
            .ToListAsync(cancellationToken);

        return assignments;
    }
}