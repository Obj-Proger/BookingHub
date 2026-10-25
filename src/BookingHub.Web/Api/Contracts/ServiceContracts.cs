namespace BookingHub.Web.Api.Contracts;

public sealed record ServiceResponse(
    Guid ServiceId, string Name, TimeSpan Duration, decimal BasePriceAmount, string BasePriceCurrency,
    TimeSpan BufferBefore, TimeSpan BufferAfter, string Color);

public sealed record ServiceCreatedResponse(Guid ServiceId, string Name);

public sealed record CreateServiceRequest(
    string? Name, TimeSpan Duration, decimal BasePriceAmount, string? BasePriceCurrency,
    TimeSpan BufferBefore, TimeSpan BufferAfter, string? Color);

public sealed record RenameServiceRequest(string? NewName);

public sealed record UpdateServicePricingRequest(decimal NewAmount, string? NewCurrency);

public sealed record UpdateServiceDurationRequest(TimeSpan NewDuration);

public sealed record UpdateServiceBuffersRequest(TimeSpan NewBufferBefore, TimeSpan NewBufferAfter);

public sealed record UpdateServiceColorRequest(string? NewColor);