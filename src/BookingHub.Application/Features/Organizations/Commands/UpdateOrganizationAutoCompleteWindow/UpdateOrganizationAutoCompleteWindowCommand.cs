using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationAutoCompleteWindow;

public sealed record UpdateOrganizationAutoCompleteWindowCommand(Guid OrganizationId, TimeSpan Window)
    : ICommand, IRequireOrganizationManagement;