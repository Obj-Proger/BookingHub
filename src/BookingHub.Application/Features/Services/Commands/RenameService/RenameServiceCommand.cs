using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.RenameService;

public sealed record RenameServiceCommand(Guid OrganizationId, Guid ServiceId, string? NewName)
    : ICommand, IRequireOrganizationManagement;