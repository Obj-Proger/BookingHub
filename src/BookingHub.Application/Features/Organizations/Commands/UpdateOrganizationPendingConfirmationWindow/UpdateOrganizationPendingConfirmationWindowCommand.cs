using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationPendingConfirmationWindow;

public sealed record UpdateOrganizationPendingConfirmationWindowCommand(Guid OrganizationId, TimeSpan Window)
    : ICommand, IRequireOrganizationManagement;