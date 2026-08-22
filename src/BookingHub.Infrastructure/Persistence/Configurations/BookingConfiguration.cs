using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(b => b.CancellationReason);

        builder.ComplexProperty(b => b.ClientContact, contact =>
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

        builder.ComplexProperty(b => b.TimeSlot, slot =>
        {
            slot.Property(s => s.StartUtc).HasColumnName("TimeSlot_StartUtc").IsRequired();
            slot.Property(s => s.EndUtc).HasColumnName("TimeSlot_EndUtc").IsRequired();
        });

        builder.ComplexProperty(b => b.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3).IsRequired();
        });

        builder.ComplexProperty(b => b.ConfirmationToken, token =>
        {
            token.Property(t => t.Value).HasColumnName("ConfirmationToken").HasMaxLength(64).IsRequired();
        });

        builder.ComplexProperty(b => b.CancellationToken, token =>
        {
            token.Property(t => t.Value).HasColumnName("CancellationToken").HasMaxLength(64).IsRequired();
        });

        builder.HasOne<Organization>().WithMany().HasForeignKey(b => b.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>().WithMany().HasForeignKey(b => b.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(b => b.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(b => b.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Client>().WithMany().HasForeignKey(b => b.ClientId).OnDelete(DeleteBehavior.Restrict);

        // Supports AvailabilityContextLoader's occupied-window lookup (EmployeeId + Status,
        // exact overlap is then narrowed further by the EXCLUDE constraint's own GiST index).
        builder.HasIndex(b => new { b.EmployeeId, b.Status });

        // Supports GetAnalyticsDashboardQuery's OrganizationId (+ optional LocationId) + date-range filter.
        builder.HasIndex(b => new { b.OrganizationId, b.LocationId });

        // Supports GetClientProfileQuery.
        builder.HasIndex(b => new { b.OrganizationId, b.ClientId });

        // Supports ConfirmBookingCommandHandler's cascading confirmation of series siblings.
        builder.HasIndex(b => b.RecurringSeriesId);
    }
}