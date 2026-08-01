using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationMemberRepository
{
    void Add(OrganizationMember member);
    Task<OrganizationMember?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
}