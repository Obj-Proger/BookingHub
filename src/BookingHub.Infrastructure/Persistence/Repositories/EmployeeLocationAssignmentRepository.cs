using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeLocationAssignmentRepository(ApplicationDbContext dbContext) : IEmployeeLocationAssignmentRepository
{
    public void Add(EmployeeLocationAssignment assignment) => dbContext.EmployeeLocationAssignments.Add(assignment);

    public Task<EmployeeLocationAssignment?> GetByIdAsync(Guid locationId, Guid assignmentId, CancellationToken cancellationToken) =>
        dbContext.EmployeeLocationAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId && a.LocationId == locationId, cancellationToken);
}