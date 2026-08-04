using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IBookingRepository
{
    void Add(Booking booking);
}