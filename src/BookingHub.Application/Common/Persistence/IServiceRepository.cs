using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IServiceRepository
{
    void Add(Service service);
    Task<Service?> GetByIdAsync(Guid organizationId, Guid serviceId, CancellationToken cancellationToken);
}