using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceDuration;

public sealed record UpdateServiceDurationCommand(Guid OrganizationId, Guid ServiceId, TimeSpan NewDuration)
    : ICommand, IRequireOrganizationManagement;