using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class LocationsApiClient(AuthorizedApiClient authorizedClient) : ILocationsApiClient
{
    private static string BasePath(Guid organizationId) => $"api/v1/organizations/{organizationId}/locations";

    public Task<ApiResult<IReadOnlyList<LocationResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<LocationResponse>>(BasePath(organizationId), cancellationToken);

    public Task<ApiResult<LocationResponse>> GetAsync(Guid organizationId, Guid locationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<LocationResponse>($"{BasePath(organizationId)}/{locationId}", cancellationToken);

    public Task<ApiResult<LocationCreatedResponse>> CreateAsync(
        Guid organizationId, string? name, string? address, string? timeZone,
        IReadOnlyList<DailyHoursDto> workingHours, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<LocationCreatedResponse>(
            HttpMethod.Post, BasePath(organizationId),
            new CreateLocationRequest(name, address, timeZone, workingHours), cancellationToken);

    public Task<ApiResult> RenameAsync(Guid organizationId, Guid locationId, string? newName, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{locationId}/name", new RenameLocationRequest(newName), cancellationToken);

    public Task<ApiResult> UpdateAddressAsync(Guid organizationId, Guid locationId, string? newAddress, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{locationId}/address",
            new UpdateLocationAddressRequest(newAddress), cancellationToken);

    public Task<ApiResult> UpdateWorkingHoursAsync(
        Guid organizationId, Guid locationId, IReadOnlyList<DailyHoursDto> workingHours, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/{locationId}/working-hours",
            new UpdateLocationWorkingHoursRequest(workingHours), cancellationToken);
}