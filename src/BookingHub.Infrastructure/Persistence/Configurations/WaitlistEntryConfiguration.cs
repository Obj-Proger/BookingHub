using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ConfigureBaseEntity();

        builder.ComplexProperty(e => e.ClientContact, contact =>
        {
            contact.ComplexProperty(c => c.Phone, phone =>
            {
                phone.Property(p => p.Value).HasColumnName("ClientPhone").HasMaxLength(20).IsRequired();
            });

            contact.Property(c => c.Name).HasColumnName("ClientName").HasMaxLength(200);

            contact.ComplexProperty(c => c.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("ClientEmail").HasMaxLength(320);
            });
        });

        builder.ComplexProperty(e => e.DesiredWindow, window =>
        {
            window.Property(w => w.StartUtc).HasColumnName("DesiredWindow_StartUtc").IsRequired();
            window.Property(w => w.EndUtc).HasColumnName("DesiredWindow_EndUtc").IsRequired();
        });

        // OfferedSlot is nullable as a whole (only set once Offer() has been called) —
        // an optional complex type still needs at least one required property inside it once
        // present, which both StartUtc/EndUtc already are.
        builder.ComplexProperty(e => e.OfferedSlot, slot =>
        {
            slot.Property(s => s.StartUtc).HasColumnName("OfferedSlot_StartUtc");
            slot.Property(s => s.EndUtc).HasColumnName("OfferedSlot_EndUtc");
        });

        builder.ComplexProperty(e => e.ManagementToken, token =>
        {
            token.Property(t => t.Value).HasColumnName("ManagementToken").HasMaxLength(64).IsRequired();
        });

        builder.HasOne<Organization>().WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>().WithMany().HasForeignKey(e => e.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .HasConstraintName("FK_WaitlistEntries_RequestedEmployee")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(e => e.OfferedEmployeeId)
            .HasConstraintName("FK_WaitlistEntries_OfferedEmployee")
            .OnDelete(DeleteBehavior.Restrict);

        // Supports WaitlistOfferService's candidate lookup (organization + location + service +
        // Waiting status, ordered by CreatedAtUtc for FIFO — see IAuditable, Domain patch).
        builder.HasIndex(e => new { e.OrganizationId, e.LocationId, e.ServiceId, e.Status });
    }
}