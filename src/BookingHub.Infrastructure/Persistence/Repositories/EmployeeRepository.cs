using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public void Add(Employee employee) => dbContext.Employees.Add(employee);

    public Task<Employee?> GetByIdAsync(Guid organizationId, Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.OrganizationId == organizationId, cancellationToken);
}