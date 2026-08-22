using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.HasOne<Organization>().WithMany().HasForeignKey(r => r.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>().WithMany().HasForeignKey(r => r.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Booking>().WithMany().HasForeignKey(r => r.BookingId).OnDelete(DeleteBehavior.Restrict);

        // The long-standing debt from Application (SubmitReviewCommandHandler's ExistsForBookingAsync
        // pre-check) — final guarantee against a race between two submissions for the same booking.
        builder.HasIndex(r => r.BookingId).IsUnique();

        // Supports GetEmployeeReviewsQuery (OrganizationId + EmployeeId + !IsHidden filter).
        builder.HasIndex(r => new { r.OrganizationId, r.EmployeeId, r.IsHidden });
    }
}