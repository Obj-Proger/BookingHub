using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployee;

internal sealed class GetEmployeeQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetEmployeeQuery, EmployeeResponse>
{
    public async Task<Result<EmployeeResponse>> Handle(GetEmployeeQuery query, CancellationToken cancellationToken)
    {
        var response = await dbContext.Employees
            .Where(e => e.Id == query.EmployeeId && e.OrganizationId == query.OrganizationId)
            .Select(e => new EmployeeResponse(e.Id, e.FullName, e.PhotoUrl, e.IsBookable))
            .FirstOrDefaultAsync(cancellationToken);

        return response is not null ? response : Result.Failure<EmployeeResponse>(ApplicationErrors.Employee.NotFound);
    }
}