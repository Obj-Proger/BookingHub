using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationMemberRepository
{
    void Add(OrganizationMember member);
    void Remove(OrganizationMember member);
    Task<OrganizationMember?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);

    /// <param name="organizationId">Filtered alongside <paramref name="organizationMemberId"/> — same rule as every other location/organization-scoped lookup in this project.</param>
    Task<OrganizationMember?> GetByIdAsync(Guid organizationId, Guid organizationMemberId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Used to guard against leaving an organization with zero Owners.</summary>
    Task<bool> AnyOtherOwnerExistsAsync(Guid organizationId, Guid excludingMemberId, CancellationToken cancellationToken);
}