using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Organizations.Queries.GetMyOrganizationMemberships;

internal sealed class GetMyOrganizationMembershipsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IQueryHandler<GetMyOrganizationMembershipsQuery, IReadOnlyList<MyOrganizationMembershipResponse>>
{
    public async Task<Result<IReadOnlyList<MyOrganizationMembershipResponse>>> Handle(
        GetMyOrganizationMembershipsQuery query, CancellationToken cancellationToken)
    {
        var memberships = await (
            from m in dbContext.OrganizationMembers
            join o in dbContext.Organizations on m.OrganizationId equals o.Id
            where m.UserId == currentUser.UserId
            select new MyOrganizationMembershipResponse(o.Id, o.Name, m.Role, m.LocationId, m.EmployeeId))
            .ToListAsync(cancellationToken);

        return memberships;
    }
}