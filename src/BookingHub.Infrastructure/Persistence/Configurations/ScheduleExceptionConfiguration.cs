using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ConfigureBaseEntity();

        builder.HasOne<EmployeeLocationAssignment>().WithMany().HasForeignKey(e => e.EmployeeLocationAssignmentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.EmployeeLocationAssignmentId, e.Date }).IsUnique();
    }
}