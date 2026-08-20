using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.TimeZone).IsRequired();

        builder.ComplexProperty(l => l.Address, address =>
        {
            address.Property(a => a.Value).HasColumnName("Address").HasMaxLength(500).IsRequired();
        });

        builder.ComplexProperty(l => l.WorkingHours, workingHours =>
        {
            workingHours.ToJson();
            workingHours.ComplexCollection(w => w.Days);
        });

        builder.HasIndex(l => l.OrganizationId);
    }
}