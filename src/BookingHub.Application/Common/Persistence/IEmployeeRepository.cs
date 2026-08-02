using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IEmployeeRepository
{
    void Add(Employee employee);
    Task<Employee?> GetByIdAsync(Guid organizationId, Guid employeeId, CancellationToken cancellationToken);
}