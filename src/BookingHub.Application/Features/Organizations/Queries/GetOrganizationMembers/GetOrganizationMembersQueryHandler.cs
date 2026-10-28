using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Organizations.Queries.GetOrganizationMembers;

internal sealed class GetOrganizationMembersQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    : IQueryHandler<GetOrganizationMembersQuery, IReadOnlyList<OrganizationMemberResponse>>
{
    public async Task<Result<IReadOnlyList<OrganizationMemberResponse>>> Handle(GetOrganizationMembersQuery query, CancellationToken cancellationToken)
    {
        var members = await dbContext.OrganizationMembers
            .Where(m => m.OrganizationId == query.OrganizationId)
            .Select(m => new { m.Id, m.UserId, m.Role, m.LocationId, m.EmployeeId })
            .ToListAsync(cancellationToken);

        var emails = await userDirectory.GetEmailsAsync([.. members.Select(m => m.UserId)], cancellationToken);

        return members
            .Select(m => new OrganizationMemberResponse(m.Id, m.UserId, emails.GetValueOrDefault(m.UserId), m.Role, m.LocationId, m.EmployeeId))
            .ToList();
    }
}