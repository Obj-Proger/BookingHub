namespace BookingHub.Domain.Entities;

public sealed class Employee : BaseEntity
{
    public const int MaxFullNameLength = 200;
    public const int MaxPhotoUrlLength = 2048;

    public Guid OrganizationId { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullName { get; private set; } = null!;
    public string? PhotoUrl { get; private set; }
    public bool IsBookable { get; private set; }

    private Employee(Guid id, Guid organizationId, string fullName) : base(id)
    {
        OrganizationId = organizationId;
        FullName = fullName;
        IsBookable = true;
    }

    private Employee()
    {
    }

    public static Result<Employee> Create(Guid organizationId, string? fullName)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, DomainErrors.Employee.OrganizationIdEmpty);
        if (organizationIdResult.IsFailure)
            return Result.Failure<Employee>(organizationIdResult.Error);

        var nameResult = Guard.RequiredText(fullName, MaxFullNameLength, DomainErrors.Employee.FullNameEmpty, DomainErrors.Employee.FullNameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure<Employee>(nameResult.Error);

        return new Employee(Guid.CreateVersion7(), organizationId, nameResult.Value);
    }

    public Result Rename(string? newFullName)
    {
        var nameResult = Guard.RequiredText(newFullName, MaxFullNameLength, DomainErrors.Employee.FullNameEmpty, DomainErrors.Employee.FullNameTooLong);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        FullName = nameResult.Value;
        return Result.Success();
    }

    /// <summary>Controls whether this employee appears as selectable on the public booking page.</summary>
    public void SetBookable(bool isBookable) => IsBookable = isBookable;

    public Result UpdatePhoto(string? photoUrl)
    {
        if (photoUrl is null)
        {
            PhotoUrl = null;
            return Result.Success();
        }

        var trimmed = photoUrl.Trim();
        if (!IsWebUrl(trimmed))
            return Result.Failure(DomainErrors.Employee.InvalidPhotoUrl);

        PhotoUrl = trimmed;
        return Result.Success();
    }

    public Result LinkUser(Guid userId)
    {
        var userIdResult = Guard.NotEmpty(userId, DomainErrors.Employee.UserIdEmpty);
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        if (UserId is not null && UserId != userId)
            return Result.Failure(DomainErrors.Employee.AlreadyLinkedToDifferentUser);

        UserId = userId;
        return Result.Success();
    }

    /// <summary>Only http(s) links are accepted: the URL ends up in an <c>&lt;img src&gt;</c> on staff and guest pages.</summary>
    private static bool IsWebUrl(string url) =>
        url.Length <= MaxPhotoUrlLength
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}