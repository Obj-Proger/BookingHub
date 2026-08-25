using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class LocationServiceOverrideRepository(ApplicationDbContext dbContext) : ILocationServiceOverrideRepository
{
    public void Add(LocationServiceOverride @override) => dbContext.LocationServiceOverrides.Add(@override);
    public void Remove(LocationServiceOverride @override) => dbContext.LocationServiceOverrides.Remove(@override);

    public Task<LocationServiceOverride?> GetByIdAsync(Guid locationId, Guid overrideId, CancellationToken cancellationToken) =>
        dbContext.LocationServiceOverrides.FirstOrDefaultAsync(o => o.Id == overrideId && o.LocationId == locationId, cancellationToken);

    public Task<bool> ExistsForServiceAsync(Guid locationId, Guid serviceId, CancellationToken cancellationToken) =>
        dbContext.LocationServiceOverrides.AnyAsync(o => o.LocationId == locationId && o.ServiceId == serviceId, cancellationToken);
}