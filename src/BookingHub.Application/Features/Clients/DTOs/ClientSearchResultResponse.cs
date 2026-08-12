namespace BookingHub.Application.Features.Clients.DTOs;

public sealed record ClientSearchResultResponse(Guid ClientId, string Phone, string? Name);