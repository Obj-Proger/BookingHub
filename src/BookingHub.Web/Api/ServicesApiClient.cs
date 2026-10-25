using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class ServicesApiClient(AuthorizedApiClient authorizedClient) : IServicesApiClient
{
    private static string BasePath(Guid organizationId) => $"api/v1/organizations/{organizationId}/services";

    private static string OverridesPath(Guid organizationId, Guid locationId) =>
        $"api/v1/organizations/{organizationId}/locations/{locationId}/service-overrides";

    public Task<ApiResult<IReadOnlyList<ServiceResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<ServiceResponse>>(BasePath(organizationId), cancellationToken);

    public Task<ApiResult<ServiceResponse>> GetAsync(Guid organizationId, Guid serviceId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<ServiceResponse>($"{BasePath(organizationId)}/{serviceId}", cancellationToken);

    public Task<ApiResult<ServiceCreatedResponse>> CreateAsync(
        Guid organizationId, string? name, TimeSpan duration, decimal basePriceAmount, string? basePriceCurrency,
        TimeSpan bufferBefore, TimeSpan bufferAfter, string? color, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<ServiceCreatedResponse>(
            HttpMethod.Post, BasePath(organizationId),
            new CreateServiceRequest(name, duration, basePriceAmount, basePriceCurrency, bufferBefore, bufferAfter, color),
            cancellationToken);

    public Task<ApiResult> RenameAsync(Guid organizationId, Guid serviceId, string? newName, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{serviceId}/name", new RenameServiceRequest(newName), cancellationToken);

    public Task<ApiResult> UpdatePricingAsync(
        Guid organizationId, Guid serviceId, decimal amount, string? currency, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{serviceId}/pricing",
            new UpdateServicePricingRequest(amount, currency), cancellationToken);

    public Task<ApiResult> UpdateDurationAsync(Guid organizationId, Guid serviceId, TimeSpan duration, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{serviceId}/duration",
            new UpdateServiceDurationRequest(duration), cancellationToken);

    public Task<ApiResult> UpdateBuffersAsync(
        Guid organizationId, Guid serviceId, TimeSpan bufferBefore, TimeSpan bufferAfter, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{serviceId}/buffers",
            new UpdateServiceBuffersRequest(bufferBefore, bufferAfter), cancellationToken);

    public Task<ApiResult> UpdateColorAsync(Guid organizationId, Guid serviceId, string? color, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{serviceId}/color", new UpdateServiceColorRequest(color), cancellationToken);

    public Task<ApiResult<IReadOnlyList<LocationServiceOverrideResponse>>> GetLocationOverridesAsync(
        Guid organizationId, Guid locationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<LocationServiceOverrideResponse>>(
            OverridesPath(organizationId, locationId), cancellationToken);

    public Task<ApiResult<LocationServiceOverrideCreatedResponse>> CreateLocationOverrideAsync(
        Guid organizationId, Guid locationId, Guid serviceId, decimal amount, string? currency, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<LocationServiceOverrideCreatedResponse>(
            HttpMethod.Post, OverridesPath(organizationId, locationId),
            new CreateLocationServiceOverrideRequest(serviceId, amount, currency), cancellationToken);

    public Task<ApiResult> UpdateLocationOverridePriceAsync(
        Guid organizationId, Guid locationId, Guid overrideId, decimal amount, string? currency, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{OverridesPath(organizationId, locationId)}/{overrideId}",
            new UpdateLocationServiceOverridePriceRequest(amount, currency), cancellationToken);

    public Task<ApiResult> DeleteLocationOverrideAsync(
        Guid organizationId, Guid locationId, Guid overrideId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Delete, $"{OverridesPath(organizationId, locationId)}/{overrideId}", body: null, cancellationToken);
}