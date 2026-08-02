using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface ILocationServiceOverrideRepository
{
    void Add(LocationServiceOverride @override);
    void Remove(LocationServiceOverride @override);

    /// <param name="locationId">Filtered alongside <paramref name="overrideId"/> — same rule as
    /// <see cref="IEmployeeLocationAssignmentRepository"/>.</param>
    Task<LocationServiceOverride?> GetByIdAsync(Guid locationId, Guid overrideId, CancellationToken cancellationToken);

    Task<bool> ExistsForServiceAsync(Guid locationId, Guid serviceId, CancellationToken cancellationToken);
}