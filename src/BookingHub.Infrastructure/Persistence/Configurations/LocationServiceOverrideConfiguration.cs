using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class LocationServiceOverrideConfiguration : IEntityTypeConfiguration<LocationServiceOverride>
{
    public void Configure(EntityTypeBuilder<LocationServiceOverride> builder)
    {
        builder.ConfigureBaseEntity();

        builder.ComplexProperty(o => o.OverridePrice, price =>
        {
            price.Property(p => p.Amount).HasColumnName("OverridePriceAmount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("OverridePriceCurrency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne<Location>().WithMany().HasForeignKey(o => o.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(o => o.ServiceId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.LocationId, o.ServiceId }).IsUnique();
    }
}