using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IEmployeesApiClient
{
    Task<ApiResult<IReadOnlyList<EmployeeResponse>>> GetAllAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<ApiResult<EmployeeResponse>> GetAsync(Guid organizationId, Guid employeeId, CancellationToken cancellationToken);

    Task<ApiResult<EmployeeCreatedResponse>> CreateAsync(Guid organizationId, string? fullName, CancellationToken cancellationToken);

    Task<ApiResult> RenameAsync(Guid organizationId, Guid employeeId, string? newFullName, CancellationToken cancellationToken);

    Task<ApiResult> SetBookableAsync(Guid organizationId, Guid employeeId, bool isBookable, CancellationToken cancellationToken);

    /// <param name="photoUrl">Null removes the photo.</param>
    Task<ApiResult> UpdatePhotoAsync(Guid organizationId, Guid employeeId, string? photoUrl, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<EmployeeAssignmentResponse>>> GetAssignmentsAsync(
        Guid organizationId, Guid employeeId, CancellationToken cancellationToken);

    Task<ApiResult<EmployeeAssignmentCreatedResponse>> AssignToLocationAsync(
        Guid organizationId, Guid locationId, Guid employeeId, CancellationToken cancellationToken);

    Task<ApiResult> DeactivateAssignmentAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, CancellationToken cancellationToken);
}