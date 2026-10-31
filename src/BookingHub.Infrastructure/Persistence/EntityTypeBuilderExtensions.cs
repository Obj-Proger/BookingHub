using BookingHub.Domain.Common;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// Mappings shared by several entity configurations. Column lengths always come from the
/// limits the Domain itself enforces, so the schema and the validation cannot drift apart.
/// </summary>
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

    /// <summary>Maps <see cref="Money"/> to the columns <c>{prefix}Amount</c> and <c>{prefix}Currency</c>.</summary>
    public static void MapMoney(this ComplexPropertyBuilder<Money> money, string columnPrefix)
    {
        money.Property(m => m.Amount).HasColumnName($"{columnPrefix}Amount").HasPrecision(18, 2);
        money.Property(m => m.Currency).HasColumnName($"{columnPrefix}Currency").HasMaxLength(Money.CurrencyCodeLength).IsRequired();
    }

    public static void MapSecurityToken(this ComplexPropertyBuilder<SecurityToken> token, string columnName) =>
        token.Property(t => t.Value).HasColumnName(columnName).HasMaxLength(SecurityToken.EncodedLength).IsRequired();

    public static void MapPhoneNumber(this ComplexPropertyBuilder<PhoneNumber> phone, string columnName) =>
        phone.Property(p => p.Value).HasColumnName(columnName).HasMaxLength(PhoneNumber.MaxLength).IsRequired();

    /// <summary>Maps the contact snapshot stored with a booking or waitlist entry to <c>ClientPhone/ClientName/ClientEmail</c>.</summary>
    public static void MapClientContact(this ComplexPropertyBuilder<ClientContact> contact)
    {
        contact.ComplexProperty(c => c.Phone, phone => phone.MapPhoneNumber("ClientPhone"));

        contact.Property(c => c.Name).HasColumnName("ClientName").HasMaxLength(ClientContact.MaxNameLength);

        contact.ComplexProperty(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("ClientEmail").HasMaxLength(Email.MaxLength);
        });
    }
}