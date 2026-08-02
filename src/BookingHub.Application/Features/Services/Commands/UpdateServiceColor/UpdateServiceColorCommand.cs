using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceColor;

public sealed record UpdateServiceColorCommand(Guid OrganizationId, Guid ServiceId, string? NewColor)
    : ICommand, IRequireOrganizationManagement;