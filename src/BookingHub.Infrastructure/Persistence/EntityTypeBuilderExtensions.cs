using BookingHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence;

internal static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Every entity in this project generates its own Id client-side (<c>Guid.CreateVersion7()</c>,
    /// see BaseEntity) — <c>ValueGeneratedNever</c> stops EF Core from assuming a database-generated
    /// key and silently overwriting it, which is the default EF Core convention for Guid keys.
    /// </summary>
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
    }
}