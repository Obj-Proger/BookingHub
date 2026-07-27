namespace BookingHub.Domain.Entities;

public sealed class Employee : BaseEntity
{
    private const int MaxFullNameLength = 200;

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

        if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out _))
            return Result.Failure(DomainErrors.Employee.InvalidPhotoUrl);

        PhotoUrl = photoUrl;
        return Result.Success();
    }

    public Result LinkUser(Guid userId)
    {
        if (UserId is not null && UserId != userId)
            return Result.Failure(DomainErrors.Employee.AlreadyLinkedToDifferentUser);

        UserId = userId;
        return Result.Success();
    }
}