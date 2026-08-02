namespace BookingHub.Application.Features.Services.DTOs;

public sealed record LocationServiceOverrideCreatedResponse(
    Guid OverrideId, Guid LocationId, Guid ServiceId, decimal OverridePriceAmount, string OverridePriceCurrency);