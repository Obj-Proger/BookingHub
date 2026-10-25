namespace BookingHub.Web.Api.Contracts;

public sealed record LocationServiceOverrideResponse(
    Guid OverrideId, Guid ServiceId, decimal OverridePriceAmount, string OverridePriceCurrency);

public sealed record LocationServiceOverrideCreatedResponse(
    Guid OverrideId, Guid LocationId, Guid ServiceId, decimal OverridePriceAmount, string OverridePriceCurrency);

public sealed record CreateLocationServiceOverrideRequest(Guid ServiceId, decimal OverridePriceAmount, string? OverridePriceCurrency);

public sealed record UpdateLocationServiceOverridePriceRequest(decimal NewAmount, string? NewCurrency);