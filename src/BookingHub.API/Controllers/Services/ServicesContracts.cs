namespace BookingHub.API.Controllers.Services;

/// <param name="Duration">.NET TimeSpan format (e.g. "00:30:00" for 30 minutes).</param>
/// <param name="BufferBefore">Same format as <paramref name="Duration"/>.</param>
/// <param name="BufferAfter">Same format as <paramref name="Duration"/>.</param>
public sealed record CreateServiceRequest(
    string? Name, TimeSpan Duration, decimal BasePriceAmount, string? BasePriceCurrency,
    TimeSpan BufferBefore, TimeSpan BufferAfter, string? Color);

public sealed record RenameServiceRequest(string? NewName);
public sealed record UpdateServicePricingRequest(decimal NewAmount, string? NewCurrency);
public sealed record UpdateServiceDurationRequest(TimeSpan NewDuration);
public sealed record UpdateServiceBuffersRequest(TimeSpan NewBufferBefore, TimeSpan NewBufferAfter);
public sealed record UpdateServiceColorRequest(string? NewColor);

public sealed record CreateLocationServiceOverrideRequest(Guid ServiceId, decimal OverridePriceAmount, string? OverridePriceCurrency);
public sealed record UpdateLocationServiceOverridePriceRequest(decimal NewAmount, string? NewCurrency);