using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationRepository
{
    void Add(Organization organization);
    void Remove(Organization organization);
    Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes every row belonging to this organization across all org-scoped tables, in FK-safe
    /// order, via bulk ExecuteDeleteAsync (no entities are loaded into memory). Does NOT delete the
    /// Organization row itself — call Remove for that. Does NOT touch Client rows — clients are
    /// global, shared across organizations. Runs inside whatever transaction is already open for
    /// the request (TenantResolutionMiddleware opens one whenever the route carries an
    /// organizationId), so no transaction is started here.
    /// </summary>
    Task DeleteAllDataAsync(Guid organizationId, CancellationToken cancellationToken);
}