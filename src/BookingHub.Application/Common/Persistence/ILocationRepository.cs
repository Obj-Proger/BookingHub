using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface ILocationRepository
{
    void Add(Location location);

    /// <param name="organizationId">Filtered alongside <paramref name="locationId"/> so a location
    /// can never be loaded through the wrong organization's scope.</param>
    Task<Location?> GetByIdAsync(Guid organizationId, Guid locationId, CancellationToken cancellationToken);
}