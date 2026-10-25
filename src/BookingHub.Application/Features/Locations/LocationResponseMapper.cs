using BookingHub.Application.Features.Locations.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Locations;

internal static class LocationResponseMapper
{
    public static LocationResponse ToResponse(Location location) =>
        new(location.Id, location.Name, location.Address.Value, location.TimeZone, WeeklyHoursMapper.ToDtos(location.WorkingHours));
}