using System.Text.RegularExpressions;

namespace BookingHub.Domain.Entities;

public sealed class Organization : BaseEntity
{
    private const int MaxNameLength = 200;
    private const int MaxSlugLength = 100;
    private static readonly Regex SlugPattern = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// How many hours before a booking's start time a client may still cancel or reschedule it
    /// through their self-service link. Does not apply to staff-initiated cancellations.
    /// </summary>
    public int CancellationDeadlineHours { get; private set; }

    private Organization(Guid id, string name, string slug) : base(id)
    {
        Name = name;
        Slug = slug;
        CancellationDeadlineHours = 24;
    }

    private Organization()
    {
    }

    public static Result<Organization> Create(string? name, string? slug)
    {
        var nameResult = Guard.RequiredText(name, MaxNameLength, DomainErrors.Organization.NameEmpty, DomainErrors.Organization.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure<Organization>(nameResult.Error);

        var slugResult = ValidateSlug(slug);
        if (slugResult.IsFailure)
            return Result.Failure<Organization>(slugResult.Error);

        return new Organization(Guid.CreateVersion7(), nameResult.Value, slugResult.Value);
    }

    public Result Rename(string? newName)
    {
        var nameResult = Guard.RequiredText(newName, MaxNameLength, DomainErrors.Organization.NameEmpty, DomainErrors.Organization.NameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;
        return Result.Success();
    }

    public Result UpdateCancellationDeadline(int hours)
    {
        if (hours < 0)
            return Result.Failure(DomainErrors.Organization.CancellationDeadlineNegative);

        CancellationDeadlineHours = hours;
        return Result.Success();
    }

    private static Result<string> ValidateSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Result.Failure<string>(DomainErrors.Organization.SlugEmpty);

        var trimmed = slug.Trim();

        return trimmed.Length > MaxSlugLength || !SlugPattern.IsMatch(trimmed)
            ? Result.Failure<string>(DomainErrors.Organization.SlugInvalidFormat)
            : trimmed;
    }
}
