using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IServicesApiClient
{
    Task<ApiResult<IReadOnlyList<ServiceResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<ApiResult<ServiceResponse>> GetAsync(Guid organizationId, Guid serviceId, CancellationToken cancellationToken);

    Task<ApiResult<ServiceCreatedResponse>> CreateAsync(
        Guid organizationId, string? name, TimeSpan duration, decimal basePriceAmount, string? basePriceCurrency,
        TimeSpan bufferBefore, TimeSpan bufferAfter, string? color, CancellationToken cancellationToken);

    Task<ApiResult> RenameAsync(Guid organizationId, Guid serviceId, string? newName, CancellationToken cancellationToken);

    Task<ApiResult> UpdatePricingAsync(Guid organizationId, Guid serviceId, decimal amount, string? currency, CancellationToken cancellationToken);

    Task<ApiResult> UpdateDurationAsync(Guid organizationId, Guid serviceId, TimeSpan duration, CancellationToken cancellationToken);

    Task<ApiResult> UpdateBuffersAsync(
        Guid organizationId, Guid serviceId, TimeSpan bufferBefore, TimeSpan bufferAfter, CancellationToken cancellationToken);

    Task<ApiResult> UpdateColorAsync(Guid organizationId, Guid serviceId, string? color, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<LocationServiceOverrideResponse>>> GetLocationOverridesAsync(
        Guid organizationId, Guid locationId, CancellationToken cancellationToken);

    Task<ApiResult<LocationServiceOverrideCreatedResponse>> CreateLocationOverrideAsync(
        Guid organizationId, Guid locationId, Guid serviceId, decimal amount, string? currency, CancellationToken cancellationToken);

    Task<ApiResult> UpdateLocationOverridePriceAsync(
        Guid organizationId, Guid locationId, Guid overrideId, decimal amount, string? currency, CancellationToken cancellationToken);

    Task<ApiResult> DeleteLocationOverrideAsync(Guid organizationId, Guid locationId, Guid overrideId, CancellationToken cancellationToken);
}