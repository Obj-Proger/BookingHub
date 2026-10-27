using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IClientsApiClient
{
    /// <summary>Finds a client who has visited the organization; a 404 error means there is none with this number.</summary>
    Task<ApiResult<ClientSearchResultResponse>> SearchByPhoneAsync(Guid organizationId, string phone, CancellationToken cancellationToken);

    Task<ApiResult<ClientProfileResponse>> GetProfileAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken);
}