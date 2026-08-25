using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationMemberRepository(ApplicationDbContext dbContext) : IOrganizationMemberRepository
{
    public void Add(OrganizationMember member) => dbContext.OrganizationMembers.Add(member);
    public void Remove(OrganizationMember member) => dbContext.OrganizationMembers.Remove(member);

    public Task<OrganizationMember?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        dbContext.OrganizationMembers.FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);

    public Task<OrganizationMember?> GetByIdAsync(Guid organizationId, Guid organizationMemberId, CancellationToken cancellationToken) =>
        dbContext.OrganizationMembers.FirstOrDefaultAsync(m => m.Id == organizationMemberId && m.OrganizationId == organizationId, cancellationToken);

    public Task<bool> ExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        dbContext.OrganizationMembers.AnyAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);

    public Task<bool> AnyOtherOwnerExistsAsync(Guid organizationId, Guid excludingMemberId, CancellationToken cancellationToken) =>
        dbContext.OrganizationMembers.AnyAsync(
            m => m.OrganizationId == organizationId && m.Id != excludingMemberId && m.Role == OrganizationRole.Owner,
            cancellationToken);
}