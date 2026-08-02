using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IEmployeeLocationAssignmentRepository
{
    void Add(EmployeeLocationAssignment assignment);

    /// <param name="locationId">Filtered alongside <paramref name="assignmentId"/> — the same rule as
    /// <see cref="ILocationRepository"/>, applied one level deeper: authorization for this command is
    /// established against a LocationId, so loading must be scoped by that exact same LocationId.</param>
    Task<EmployeeLocationAssignment?> GetByIdAsync(Guid locationId, Guid assignmentId, CancellationToken cancellationToken);
}