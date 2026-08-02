using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Organizations.Commands.RenameOrganization;

public sealed record RenameOrganizationCommand(Guid OrganizationId, string? NewName)
    : ICommand, IRequireOrganizationManagement;