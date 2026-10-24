using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public enum SchedulingWindow
{
    PendingConfirmation,
    AutoComplete,
    WaitlistOffer
}

public interface IOrganizationsApiClient
{
    Task<ApiResult<OrganizationCreatedResponse>> CreateAsync(string? name, string? slug, CancellationToken cancellationToken);

    Task<ApiResult<OrganizationResponse>> GetAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<ApiResult> RenameAsync(Guid organizationId, string? newName, CancellationToken cancellationToken);

    Task<ApiResult> UpdateCancellationDeadlineAsync(Guid organizationId, TimeSpan deadline, CancellationToken cancellationToken);

    Task<ApiResult> UpdateSchedulingWindowAsync(Guid organizationId, SchedulingWindow window, TimeSpan value, CancellationToken cancellationToken);

    Task<ApiResult> SetAdministratorFinancialAccessAsync(Guid organizationId, bool enabled, CancellationToken cancellationToken);

    Task<ApiResult> DeleteAsync(Guid organizationId, CancellationToken cancellationToken);
}