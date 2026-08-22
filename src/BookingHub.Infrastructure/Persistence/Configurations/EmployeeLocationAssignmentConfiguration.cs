using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeLocationAssignmentConfiguration : IEntityTypeConfiguration<EmployeeLocationAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeLocationAssignment> builder)
    {
        builder.ConfigureBaseEntity();

        builder.HasOne<Employee>().WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>().WithMany().HasForeignKey(a => a.LocationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EmployeeId, a.LocationId }).IsUnique();
    }
}