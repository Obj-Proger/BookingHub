using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(s => s.Name).HasMaxLength(Service.MaxNameLength).IsRequired();
        builder.Property(s => s.Color).HasMaxLength(Service.ColorLength).IsRequired();

        builder.ComplexProperty(s => s.BasePrice, price => price.MapMoney("BasePrice"));

        builder.HasOne<Organization>().WithMany().HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.OrganizationId);
    }
}