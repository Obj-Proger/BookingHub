using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Locations.Commands.UpdateLocationAddress;

public sealed record UpdateLocationAddressCommand(Guid OrganizationId, Guid LocationId, string? NewAddress)
    : ICommand, IRequireLocationManagement;