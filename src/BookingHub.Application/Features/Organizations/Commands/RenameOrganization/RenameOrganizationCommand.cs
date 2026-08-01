using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Organizations.Commands.RenameOrganization;

public sealed record RenameOrganizationCommand(Guid OrganizationId, string? NewName)
    : ICommand, IRequireOrganizationManagement;