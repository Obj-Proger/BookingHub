using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Common.Persistence;

public interface IClientRepository
{
    void Add(Client client);
    Task<Client?> GetByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken);
    Task<Client?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken);
}