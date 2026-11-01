using BookingHub.Domain.Entities;
using BookingHub.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Type).HasMaxLength(OutboxMessage.MaxTypeLength).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(OutboxMessage.MaxErrorLength);

        // The processor's claim query: due, unprocessed messages. Processed rows — nearly all of
        // them — stay out of the index.
        builder.HasIndex(m => m.NextAttemptOnUtc).HasFilter("\"ProcessedOnUtc\" IS NULL");

        // Cascade, unlike the Domain FKs: pending work for a deleted organization has nothing left to act on.
        builder.HasOne<Organization>().WithMany().HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}