using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).HasMaxLength(ClientContact.MaxNameLength);

        builder.ComplexProperty(c => c.Phone, phone => phone.MapPhoneNumber("Phone"));

        builder.ComplexProperty(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(Email.MaxLength);
        });

        // Global uniqueness on Phone — EF Core's HasIndex cannot express a path through a
        // complex type property (confirmed: dotnet/efcore#32578, #32350, #34794 — a documented,
        // still-open limitation, not specific to our case). Added as raw SQL directly in the
        // migration instead, on the "Phone" column this configuration names above.
        builder.HasIndex(c => c.UserId).IsUnique();
    }
}