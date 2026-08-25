using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository(ApplicationDbContext dbContext) : IServiceRepository
{
    public void Add(Service service) => dbContext.Services.Add(service);

    public Task<Service?> GetByIdAsync(Guid organizationId, Guid serviceId, CancellationToken cancellationToken) =>
        dbContext.Services.FirstOrDefaultAsync(s => s.Id == serviceId && s.OrganizationId == organizationId, cancellationToken);
}