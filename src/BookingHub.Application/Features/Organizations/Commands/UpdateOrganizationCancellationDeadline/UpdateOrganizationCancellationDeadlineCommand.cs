using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationCancellationDeadline;

public sealed record UpdateOrganizationCancellationDeadlineCommand(Guid OrganizationId, int Hours)
    : ICommand, IRequireOrganizationManagement;