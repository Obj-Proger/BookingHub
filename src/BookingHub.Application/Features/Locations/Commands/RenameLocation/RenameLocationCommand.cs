using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Locations.Commands.RenameLocation;

public sealed record RenameLocationCommand(Guid OrganizationId, Guid LocationId, string? NewName)
    : ICommand, IRequireLocationManagement;