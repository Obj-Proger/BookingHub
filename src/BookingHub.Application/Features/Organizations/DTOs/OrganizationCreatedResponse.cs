namespace BookingHub.Application.Features.Organizations.DTOs;

public sealed record OrganizationCreatedResponse(Guid OrganizationId, string Name, string Slug);