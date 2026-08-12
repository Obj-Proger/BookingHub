using System.Text.RegularExpressions;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

public sealed partial class Service : BaseEntity
{
    private const int MaxNameLength = 200;

    [GeneratedRegex(@"^#[0-9A-Fa-f]{6}$")]
    private static partial Regex ColorPattern();

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public TimeSpan Duration { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public TimeSpan BufferBefore { get; private set; }
    public TimeSpan BufferAfter { get; private set; }
    public string Color { get; private set; } = null!;

    private Service(
        Guid id, Guid organizationId, string name, TimeSpan duration, Money basePrice,
        TimeSpan bufferBefore, TimeSpan bufferAfter, string color)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Duration = duration;
        BasePrice = basePrice;
        BufferBefore = bufferBefore;
        BufferAfter = bufferAfter;
        Color = color;
    }

    private Service()
    {
    }

    public static Result<Service> Create(
        Guid organizationId, string? name, TimeSpan duration, Money basePrice,
        TimeSpan bufferBefore, TimeSpan bufferAfter, string? color)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "Service.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<Service>(organizationIdResult.Error);

        var nameResult = Guard.RequiredText(name, MaxNameLength, DomainErrors.Service.NameEmpty, DomainErrors.Service.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure<Service>(nameResult.Error);

        if (duration <= TimeSpan.Zero)
            return Result.Failure<Service>(DomainErrors.Service.DurationNotPositive);

        if (bufferBefore < TimeSpan.Zero || bufferAfter < TimeSpan.Zero)
            return Result.Failure<Service>(DomainErrors.Service.NegativeBuffer);

        var colorResult = ValidateColor(color);
        if (colorResult.IsFailure)
            return Result.Failure<Service>(colorResult.Error);

        return new Service(Guid.CreateVersion7(), organizationId, nameResult.Value, duration, basePrice, bufferBefore, bufferAfter, colorResult.Value);
    }

    public Result Rename(string? newName)
    {
        var nameResult = Guard.RequiredText(newName, MaxNameLength, DomainErrors.Service.NameEmpty, DomainErrors.Service.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;
        return Result.Success();
    }

    public void UpdatePricing(Money newBasePrice) => BasePrice = newBasePrice;

    public Result UpdateDuration(TimeSpan newDuration)
    {
        if (newDuration <= TimeSpan.Zero)
            return Result.Failure(DomainErrors.Service.DurationNotPositive);

        Duration = newDuration;
        return Result.Success();
    }

    public Result UpdateBuffers(TimeSpan bufferBefore, TimeSpan bufferAfter)
    {
        if (bufferBefore < TimeSpan.Zero || bufferAfter < TimeSpan.Zero)
            return Result.Failure(DomainErrors.Service.NegativeBuffer);

        BufferBefore = bufferBefore;
        BufferAfter = bufferAfter;
        return Result.Success();
    }

    public Result UpdateColor(string? newColor)
    {
        var colorResult = ValidateColor(newColor);
        if (colorResult.IsFailure)
            return Result.Failure(colorResult.Error);

        Color = colorResult.Value;
        return Result.Success();
    }

    private static Result<string> ValidateColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color) || !ColorPattern().IsMatch(color.Trim()))
            return Result.Failure<string>(DomainErrors.Service.InvalidColor);

        return color.Trim().ToUpperInvariant();
    }
}