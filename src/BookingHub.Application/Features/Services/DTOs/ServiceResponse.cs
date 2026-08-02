namespace BookingHub.Application.Features.Services.DTOs;

public sealed record ServiceResponse(
    Guid ServiceId, string Name, TimeSpan Duration, decimal BasePriceAmount, string BasePriceCurrency,
    TimeSpan BufferBefore, TimeSpan BufferAfter, string Color);