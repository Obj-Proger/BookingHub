using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Locations.DTOs;

namespace BookingHub.Application.Features.Locations.Commands.UpdateLocationWorkingHours;

public sealed record UpdateLocationWorkingHoursCommand(Guid OrganizationId, Guid LocationId, IReadOnlyList<DailyHoursDto> WorkingHours)
    : ICommand, IRequireLocationManagement;