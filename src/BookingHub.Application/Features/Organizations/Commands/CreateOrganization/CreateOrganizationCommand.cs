using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Commands.CreateOrganization;

/// <param name="UserId">The authenticated caller, who becomes the new organization's Owner.</param>
public sealed record CreateOrganizationCommand(string? Name, string? Slug, Guid UserId)
    : ICommand<OrganizationCreatedResponse>;