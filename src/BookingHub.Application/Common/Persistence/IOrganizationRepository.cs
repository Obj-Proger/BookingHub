using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationRepository
{
    void Add(Organization organization);
    void Remove(Organization organization);
    Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes everything that belongs to the organization, without loading it. Does not delete the
    /// organization itself (call <see cref="Remove"/> and save in the same unit of work) and never
    /// touches clients, which are shared across organizations.
    /// </summary>
    Task DeleteAllDataAsync(Guid organizationId, CancellationToken cancellationToken);
}