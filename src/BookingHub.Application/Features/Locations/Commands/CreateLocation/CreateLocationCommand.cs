using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Locations.DTOs;

namespace BookingHub.Application.Features.Locations.Commands.CreateLocation;

public sealed record CreateLocationCommand(
    Guid OrganizationId, string? Name, string? Address, string? TimeZone, IReadOnlyList<DailyHoursDto> WorkingHours)
    : ICommand<LocationCreatedResponse>, IRequireOrganizationManagement;