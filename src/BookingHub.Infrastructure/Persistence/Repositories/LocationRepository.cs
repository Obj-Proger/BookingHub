using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class LocationRepository(ApplicationDbContext dbContext) : ILocationRepository
{
    public void Add(Location location) => dbContext.Locations.Add(location);

    public Task<Location?> GetByIdAsync(Guid organizationId, Guid locationId, CancellationToken cancellationToken) =>
        dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId && l.OrganizationId == organizationId, cancellationToken);
}