using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Public.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Public.Queries.GetPublicEmployees;

internal sealed class GetPublicEmployeesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPublicEmployeesQuery, IReadOnlyList<PublicEmployeeResponse>>
{
    public async Task<Result<IReadOnlyList<PublicEmployeeResponse>>> Handle(
        GetPublicEmployeesQuery query, CancellationToken cancellationToken)
    {
        var employees = await dbContext.EmployeeLocationAssignments
            .Where(a => a.LocationId == query.LocationId && a.IsActive)
            .Join(
                dbContext.Employees.Where(e => e.OrganizationId == query.OrganizationId && e.IsBookable),
                a => a.EmployeeId, e => e.Id, (a, e) => new PublicEmployeeResponse(e.Id, e.FullName))
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);

        return employees;
    }
}