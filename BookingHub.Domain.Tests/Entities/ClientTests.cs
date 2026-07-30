namespace BookingHub.Domain.Tests.Entities;

public class ClientTests
{
    private static readonly PhoneNumber ValidPhone = PhoneNumber.Create("+14155552671").Value;

    [Fact]
    public void Create_PhoneOnly_LeavesNameAndEmailNull()
    {
        var client = Client.Create(ValidPhone);

        client.Phone.Should().Be(ValidPhone);
        client.Name.Should().BeNull();
        client.Email.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameAndEmail_SetsThem()
    {
        var email = Email.Create("jane@example.com").Value;

        var client = Client.Create(ValidPhone, "  Jane Doe  ", email);

        client.Name.Should().Be("Jane Doe");
        client.Email.Should().Be(email);
    }

    [Fact]
    public void UpdateContactInfo_FillsInMissingName()
    {
        var client = Client.Create(ValidPhone);

        client.UpdateContactInfo("Jane Doe", null);

        client.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public void UpdateContactInfo_DoesNotOverwriteExistingName()
    {
        var client = Client.Create(ValidPhone, "Original Name");

        client.UpdateContactInfo("Different Name", null);

        client.Name.Should().Be("Original Name");
    }

    [Fact]
    public void UpdateContactInfo_DoesNotOverwriteExistingEmail()
    {
        var originalEmail = Email.Create("original@example.com").Value;
        var client = Client.Create(ValidPhone, email: originalEmail);
        var newEmail = Email.Create("new@example.com").Value;

        client.UpdateContactInfo(null, newEmail);

        client.Email.Should().Be(originalEmail);
    }

    [Fact]
    public void LinkUser_NotYetLinked_Succeeds()
    {
        var client = Client.Create(ValidPhone);
        var userId = Guid.CreateVersion7();

        var result = client.LinkUser(userId);

        result.IsSuccess.Should().BeTrue();
        client.UserId.Should().Be(userId);
    }

    [Fact]
    public void LinkUser_SameUserAgain_SucceedsIdempotently()
    {
        var client = Client.Create(ValidPhone);
        var userId = Guid.CreateVersion7();
        client.LinkUser(userId);

        var result = client.LinkUser(userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void LinkUser_DifferentUser_FailsWithAlreadyLinkedError()
    {
        var client = Client.Create(ValidPhone);
        client.LinkUser(Guid.CreateVersion7());

        var result = client.LinkUser(Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Client.AlreadyLinkedToDifferentUser);
    }

    [Fact]
    public void LinkUser_EmptyGuid_FailsWithValidationError()
    {
        var client = Client.Create(ValidPhone);

        var result = client.LinkUser(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}