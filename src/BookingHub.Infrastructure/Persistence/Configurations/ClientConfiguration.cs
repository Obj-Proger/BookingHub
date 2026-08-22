using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).HasMaxLength(200);

        builder.ComplexProperty(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired();
        });

        builder.ComplexProperty(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(320);
        });

        // Global uniqueness — Client is not organization-scoped (Domain: the same person
        // can have bookings across multiple organizations under one Client record).
        builder.HasIndex(c => c.Phone.Value).IsUnique();

        builder.HasIndex(c => c.UserId).IsUnique();
    }
}