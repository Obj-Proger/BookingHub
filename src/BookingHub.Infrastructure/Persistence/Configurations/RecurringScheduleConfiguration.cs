using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class RecurringScheduleConfiguration : IEntityTypeConfiguration<RecurringSchedule>
{
    public void Configure(EntityTypeBuilder<RecurringSchedule> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(s => s.DayOfWeek).HasConversion<string>();

        builder.HasOne<EmployeeLocationAssignment>().WithMany().HasForeignKey(s => s.EmployeeLocationAssignmentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.EmployeeLocationAssignmentId, s.DayOfWeek });
    }
}