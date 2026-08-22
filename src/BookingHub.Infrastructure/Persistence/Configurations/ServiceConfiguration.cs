using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Color).HasMaxLength(7).IsRequired();

        builder.ComplexProperty(s => s.BasePrice, price =>
        {
            price.Property(p => p.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne<Organization>().WithMany().HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.OrganizationId);
    }
}