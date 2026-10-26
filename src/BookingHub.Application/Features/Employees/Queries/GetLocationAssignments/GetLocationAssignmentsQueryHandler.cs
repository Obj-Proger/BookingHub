using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetLocationAssignments;

internal sealed class GetLocationAssignmentsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationAssignmentsQuery, IReadOnlyList<LocationAssignmentResponse>>
{
    public async Task<Result<IReadOnlyList<LocationAssignmentResponse>>> Handle(
        GetLocationAssignmentsQuery query, CancellationToken cancellationToken)
    {
        var locationExists = await dbContext.Locations
            .AnyAsync(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId, cancellationToken);

        if (!locationExists)
            return Result.Failure<IReadOnlyList<LocationAssignmentResponse>>(ApplicationErrors.Location.NotFound);

        var assignments = await (
            from a in dbContext.EmployeeLocationAssignments
            join e in dbContext.Employees on a.EmployeeId equals e.Id
            where a.LocationId == query.LocationId && a.IsActive && e.OrganizationId == query.OrganizationId
            orderby e.FullName
            select new LocationAssignmentResponse(a.Id, e.Id, e.FullName))
            .ToListAsync(cancellationToken);

        return assignments;
    }
}