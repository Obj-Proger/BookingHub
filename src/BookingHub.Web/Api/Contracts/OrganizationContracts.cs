namespace BookingHub.Web.Api.Contracts;

public sealed record CreateOrganizationRequest(string? Name, string? Slug);

public sealed record OrganizationCreatedResponse(Guid OrganizationId, string Name, string Slug);