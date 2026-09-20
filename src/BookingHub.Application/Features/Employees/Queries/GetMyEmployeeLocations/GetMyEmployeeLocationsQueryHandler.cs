using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Employees.Queries.GetMyEmployeeLocations;

internal sealed class GetMyEmployeeLocationsQueryHandler(
    IApplicationDbContext dbContext, ICurrentUser currentUser, IOrganizationMemberRepository organizationMemberRepository)
    : IQueryHandler<GetMyEmployeeLocationsQuery, IReadOnlyList<EmployeeLocationResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeLocationResponse>>> Handle(
        GetMyEmployeeLocationsQuery query, CancellationToken cancellationToken)
    {
        // AuthorizationBehavior already confirmed membership — re-fetched here (same pattern as
        // GetClientProfileQueryHandler) because this handler needs the caller's own EmployeeId,
        // which the behavior's yes/no membership check doesn't expose.
        var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(query.OrganizationId, currentUser.UserId, cancellationToken);
        if (caller?.EmployeeId is null)
            return Result.Failure<IReadOnlyList<EmployeeLocationResponse>>(ApplicationErrors.Employee.CallerIsNotAnEmployee);

        var locations = await (
            from a in dbContext.EmployeeLocationAssignments
            join l in dbContext.Locations on a.LocationId equals l.Id
            where a.EmployeeId == caller.EmployeeId.Value && a.IsActive
            select new EmployeeLocationResponse(l.Id, l.Name))
            .ToListAsync(cancellationToken);

        return locations;
    }
}