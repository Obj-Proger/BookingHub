using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationRepository
{
    void Add(Organization organization);
    Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>The organization's slug — the first segment of every guest URL — or null if it does not exist.</summary>
    Task<string?> GetSlugAsync(Guid organizationId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the organization and everything that belongs to it, straight in the database: nothing is
    /// loaded and no save is needed, so run it inside the caller's transaction. Clients, which are shared
    /// across organizations, are never touched.
    /// </summary>
    Task DeleteAsync(Guid organizationId, CancellationToken cancellationToken);
}