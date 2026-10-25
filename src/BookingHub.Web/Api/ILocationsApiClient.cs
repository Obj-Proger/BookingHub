using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface ILocationsApiClient
{
    Task<ApiResult<IReadOnlyList<LocationResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<ApiResult<LocationResponse>> GetAsync(Guid organizationId, Guid locationId, CancellationToken cancellationToken);

    Task<ApiResult<LocationCreatedResponse>> CreateAsync(
        Guid organizationId, string? name, string? address, string? timeZone,
        IReadOnlyList<DailyHoursDto> workingHours, CancellationToken cancellationToken);

    Task<ApiResult> RenameAsync(Guid organizationId, Guid locationId, string? newName, CancellationToken cancellationToken);

    Task<ApiResult> UpdateAddressAsync(Guid organizationId, Guid locationId, string? newAddress, CancellationToken cancellationToken);

    Task<ApiResult> UpdateWorkingHoursAsync(
        Guid organizationId, Guid locationId, IReadOnlyList<DailyHoursDto> workingHours, CancellationToken cancellationToken);
}