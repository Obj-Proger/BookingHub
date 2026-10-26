using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployees;

internal sealed class GetEmployeesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeResponse>>> Handle(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        var employees = await dbContext.Employees
            .Where(e => e.OrganizationId == query.OrganizationId)
            .OrderBy(e => e.FullName)
            .Select(EmployeeProjections.ToResponse)
            .ToListAsync(cancellationToken);

        return employees;
    }
}