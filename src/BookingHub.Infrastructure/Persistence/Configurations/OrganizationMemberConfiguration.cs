using BookingHub.Domain.Entities;
using BookingHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ConfigureBaseEntity();

        builder.HasOne<Organization>().WithMany().HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);

        // The scope of a LocationManager / Employee membership. Application checks that the referenced
        // row belongs to the same organization; the keys guarantee it exists and cannot be deleted under it.
        builder.HasOne<Location>().WithMany().HasForeignKey(m => m.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(m => m.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.OrganizationId, m.UserId }).IsUnique();
    }
}