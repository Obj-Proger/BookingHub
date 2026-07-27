using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

/// <summary>
/// A per-location price override for a service. The row's existence is the override —
/// application code falls back to <c>Service.BasePrice</c> when no override exists
/// for a given (location, service) pair.
/// </summary>
public sealed class LocationServiceOverride : BaseEntity
{
    public Guid LocationId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Money OverridePrice { get; private set; } = null!;

    private LocationServiceOverride(Guid id, Guid locationId, Guid serviceId, Money overridePrice) : base(id)
    {
        LocationId = locationId;
        ServiceId = serviceId;
        OverridePrice = overridePrice;
    }

    private LocationServiceOverride()
    {
    }

    public static Result<LocationServiceOverride> Create(Guid locationId, Guid serviceId, Money overridePrice)
    {
        var locationIdResult = Guard.NotEmpty(locationId, "LocationServiceOverride.LocationIdEmpty", "LocationId");
        if (locationIdResult.IsFailure)
            return Result.Failure<LocationServiceOverride>(locationIdResult.Error);

        var serviceIdResult = Guard.NotEmpty(serviceId, "LocationServiceOverride.ServiceIdEmpty", "ServiceId");
        if (serviceIdResult.IsFailure)
            return Result.Failure<LocationServiceOverride>(serviceIdResult.Error);

        return new LocationServiceOverride(Guid.CreateVersion7(), locationId, serviceId, overridePrice);
    }

    public void UpdatePrice(Money newOverridePrice) => OverridePrice = newOverridePrice;
}