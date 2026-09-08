using System.Text.Json;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ConfigureBaseEntity();

        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.TimeZone).IsRequired();

        builder.ComplexProperty(l => l.Address, address =>
        {
            address.Property(a => a.Value).HasColumnName("Address").HasMaxLength(500).IsRequired();
        });

        builder.Property(l => l.WorkingHours)
            .HasConversion(
                weeklyHours => JsonSerializer.Serialize(weeklyHours.Days.Select(DailyHoursJson.FromDomain), (JsonSerializerOptions?)null),
                json => WeeklyHours.Create(
                    JsonSerializer.Deserialize<List<DailyHoursJson>>(json, (JsonSerializerOptions?)null)!.Select(d => d.ToDomain())
                ).Value)
            .HasColumnName("WorkingHours")
            .HasColumnType("jsonb");

        builder.HasIndex(l => l.OrganizationId);
    }
}