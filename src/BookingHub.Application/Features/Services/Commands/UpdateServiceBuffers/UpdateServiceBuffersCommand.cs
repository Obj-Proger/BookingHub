using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceBuffers;

public sealed record UpdateServiceBuffersCommand(Guid OrganizationId, Guid ServiceId, TimeSpan NewBufferBefore, TimeSpan NewBufferAfter)
    : ICommand, IRequireOrganizationManagement;