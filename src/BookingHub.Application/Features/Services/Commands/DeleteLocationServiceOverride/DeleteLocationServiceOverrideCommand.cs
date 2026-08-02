using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Services.Commands.DeleteLocationServiceOverride;

public sealed record DeleteLocationServiceOverrideCommand(Guid OrganizationId, Guid LocationId, Guid OverrideId)
    : ICommand, IRequireLocationManagement;