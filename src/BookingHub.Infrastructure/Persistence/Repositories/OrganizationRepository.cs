using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationRepository(ApplicationDbContext dbContext) : IOrganizationRepository
{
    public void Add(Organization organization) => dbContext.Organizations.Add(organization);

    public Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken) =>
        dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken);
}