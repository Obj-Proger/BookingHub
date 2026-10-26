using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class EmployeesApiClient(AuthorizedApiClient authorizedClient) : IEmployeesApiClient
{
    private static string BasePath(Guid organizationId) => $"api/v1/organizations/{organizationId}";

    public Task<ApiResult<IReadOnlyList<EmployeeResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<EmployeeResponse>>($"{BasePath(organizationId)}/employees", cancellationToken);

    public Task<ApiResult<EmployeeResponse>> GetAsync(Guid organizationId, Guid employeeId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<EmployeeResponse>($"{BasePath(organizationId)}/employees/{employeeId}", cancellationToken);

    public Task<ApiResult<EmployeeCreatedResponse>> CreateAsync(Guid organizationId, string? fullName, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<EmployeeCreatedResponse>(
            HttpMethod.Post, $"{BasePath(organizationId)}/employees", new CreateEmployeeRequest(fullName), cancellationToken);

    public Task<ApiResult> RenameAsync(Guid organizationId, Guid employeeId, string? newFullName, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/employees/{employeeId}/name",
            new RenameEmployeeRequest(newFullName), cancellationToken);

    public Task<ApiResult> SetBookableAsync(Guid organizationId, Guid employeeId, bool isBookable, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/employees/{employeeId}/bookable",
            new SetEmployeeBookableRequest(isBookable), cancellationToken);

    public Task<ApiResult> UpdatePhotoAsync(Guid organizationId, Guid employeeId, string? photoUrl, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath(organizationId)}/employees/{employeeId}/photo",
            new UpdateEmployeePhotoRequest(photoUrl), cancellationToken);

    public Task<ApiResult<IReadOnlyList<EmployeeAssignmentResponse>>> GetAssignmentsAsync(
        Guid organizationId, Guid employeeId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<EmployeeAssignmentResponse>>(
            $"{BasePath(organizationId)}/employees/{employeeId}/assignments", cancellationToken);

    public Task<ApiResult<EmployeeAssignmentCreatedResponse>> AssignToLocationAsync(
        Guid organizationId, Guid locationId, Guid employeeId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<EmployeeAssignmentCreatedResponse>(
            HttpMethod.Post, $"{BasePath(organizationId)}/locations/{locationId}/employees",
            new AssignEmployeeToLocationRequest(employeeId), cancellationToken);

    public Task<ApiResult> DeactivateAssignmentAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Delete, $"{BasePath(organizationId)}/locations/{locationId}/employees/{assignmentId}",
            body: null, cancellationToken);
}