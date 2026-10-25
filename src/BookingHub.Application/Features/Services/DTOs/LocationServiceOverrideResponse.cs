namespace BookingHub.Application.Features.Services.DTOs;

public sealed record LocationServiceOverrideResponse(
    Guid OverrideId, Guid ServiceId, decimal OverridePriceAmount, string OverridePriceCurrency);