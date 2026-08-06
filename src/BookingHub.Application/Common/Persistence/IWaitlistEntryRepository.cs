using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IWaitlistEntryRepository
{
    void Add(WaitlistEntry entry);
}