using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(o => o.Name).HasMaxLength(Organization.MaxNameLength).IsRequired();
        builder.Property(o => o.Slug).HasMaxLength(Organization.MaxSlugLength).IsRequired();

        builder.HasIndex(o => o.Slug).IsUnique();
    }
}