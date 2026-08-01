using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationRepository
{
    void Add(Organization organization);
    Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
}