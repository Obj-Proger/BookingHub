namespace BookingHub.Domain.Tests.Entities;

public class EmployeeTests
{
    private static readonly Guid ValidOrganizationId = Guid.CreateVersion7();

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var result = Employee.Create(ValidOrganizationId, "John Smith");

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("John Smith");
        result.Value.IsBookable.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = Employee.Create(Guid.Empty, "John Smith");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyFullName_FailsWithFullNameEmptyError(string? fullName)
    {
        var result = Employee.Create(ValidOrganizationId, fullName);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Employee.FullNameEmpty);
    }

    [Fact]
    public void Rename_ValidNewName_UpdatesFullName()
    {
        var employee = Employee.Create(ValidOrganizationId, "Old Name").Value;

        var result = employee.Rename("New Name");

        result.IsSuccess.Should().BeTrue();
        employee.FullName.Should().Be("New Name");
    }

    [Fact]
    public void SetBookable_TogglesFlag()
    {
        var employee = Employee.Create(ValidOrganizationId, "John Smith").Value;

        employee.SetBookable(false);

        employee.IsBookable.Should().BeFalse();
    }

    [Fact]
    public void UpdatePhoto_ValidAbsoluteUrl_Succeeds()
    {
        var employee = Employee.Create(ValidOrganizationId, "John Smith").Value;

        var result = employee.UpdatePhoto("https://cdn.example.com/photos/john.jpg");

        result.IsSuccess.Should().BeTrue();
        employee.PhotoUrl.Should().Be("https://cdn.example.com/photos/john.jpg");
    }

    [Fact]
    public void UpdatePhoto_RelativePath_FailsWithInvalidPhotoUrlError()
    {
        var employee = Employee.Create(ValidOrganizationId, "John Smith").Value;

        var result = employee.UpdatePhoto("/photos/john.jpg");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Employee.InvalidPhotoUrl);
    }

    [Fact]
    public void UpdatePhoto_Null_ClearsExistingPhoto()
    {
        var employee = Employee.Create(ValidOrganizationId, "John Smith").Value;
        employee.UpdatePhoto("https://cdn.example.com/photos/john.jpg");

        var result = employee.UpdatePhoto(null);

        result.IsSuccess.Should().BeTrue();
        employee.PhotoUrl.Should().BeNull();
    }

    [Fact]
    public void LinkUser_DifferentUser_FailsWithAlreadyLinkedError()
    {
        var employee = Employee.Create(ValidOrganizationId, "John Smith").Value;
        employee.LinkUser(Guid.CreateVersion7());

        var result = employee.LinkUser(Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Employee.AlreadyLinkedToDifferentUser);
    }
}