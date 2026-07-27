using System.Diagnostics.CodeAnalysis;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

public sealed class Location : BaseEntity
{
    private const int MaxNameLength = 200;

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public string TimeZone { get; private set; } = null!;
    public WeeklyHours WorkingHours { get; private set; } = null!;

    private Location(Guid id, Guid organizationId, string name, Address address, string timeZone, WeeklyHours workingHours)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Address = address;
        TimeZone = timeZone;
        WorkingHours = workingHours;
    }

    private Location()
    {
    }

    public static Result<Location> Create(
    Guid organizationId, string? name, Address address, string? timeZone, WeeklyHours workingHours)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "Location.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<Location>(organizationIdResult.Error);

        var nameResult = Guard.RequiredText(name, MaxNameLength, DomainErrors.Location.NameEmpty, DomainErrors.Location.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure<Location>(nameResult.Error);

        if (!IsValidTimeZone(timeZone))
            return Result.Failure<Location>(DomainErrors.Location.InvalidTimeZone);

        return new Location(Guid.CreateVersion7(), organizationId, nameResult.Value, address, timeZone, workingHours);
    }

    public Result Rename(string? newName)
    {
        var nameResult = Guard.RequiredText(newName, MaxNameLength, DomainErrors.Location.NameEmpty, DomainErrors.Location.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;
        return Result.Success();
    }

    public void Relocate(Address newAddress) => Address = newAddress;

    public void UpdateWorkingHours(WeeklyHours newWorkingHours) => WorkingHours = newWorkingHours;

    private static bool IsValidTimeZone([NotNullWhen(true)] string? timeZone) =>
        !string.IsNullOrWhiteSpace(timeZone) && TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out _);
}