using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(string? Name, string? Slug) : ICommand<OrganizationCreatedResponse>;