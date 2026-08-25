using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class ClientRepository(ApplicationDbContext dbContext) : IClientRepository
{
    public void Add(Client client) => dbContext.Clients.Add(client);

    public Task<Client?> GetByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken) =>
        dbContext.Clients.FirstOrDefaultAsync(c => c.Phone.Value == phone.Value, cancellationToken);

    public Task<Client?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken) =>
        dbContext.Clients.FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);
}