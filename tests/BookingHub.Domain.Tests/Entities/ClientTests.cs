namespace BookingHub.Domain.Tests.Entities;

public class ClientTests
{
    private static readonly PhoneNumber ValidPhone = PhoneNumber.Create("+14155552671").Value;

    private static ClientContact Contact(string? name = null, Email? email = null) =>
        ClientContact.Create(ValidPhone, name, email).Value;

    [Fact]
    public void Create_PhoneOnly_LeavesNameAndEmailNull()
    {
        var client = Client.Create(Contact());

        client.Phone.Should().Be(ValidPhone);
        client.Name.Should().BeNull();
        client.Email.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameAndEmail_SetsThem()
    {
        var email = Email.Create("jane@example.com").Value;

        var client = Client.Create(Contact("  Jane Doe  ", email));

        client.Name.Should().Be("Jane Doe");
        client.Email.Should().Be(email);
    }

    [Fact]
    public void UpdateContactInfo_FillsInMissingName()
    {
        var client = Client.Create(Contact());

        client.UpdateContactInfo(Contact("Jane Doe"));

        client.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public void UpdateContactInfo_DoesNotOverwriteExistingName()
    {
        var client = Client.Create(Contact("Original Name"));

        client.UpdateContactInfo(Contact("Different Name"));

        client.Name.Should().Be("Original Name");
    }

    [Fact]
    public void UpdateContactInfo_DoesNotOverwriteExistingEmail()
    {
        var originalEmail = Email.Create("original@example.com").Value;
        var client = Client.Create(Contact(email: originalEmail));
        var newEmail = Email.Create("new@example.com").Value;

        client.UpdateContactInfo(Contact(email: newEmail));

        client.Email.Should().Be(originalEmail);
    }

    [Fact]
    public void LinkUser_NotYetLinked_Succeeds()
    {
        var client = Client.Create(Contact());
        var userId = Guid.CreateVersion7();

        var result = client.LinkUser(userId);

        result.IsSuccess.Should().BeTrue();
        client.UserId.Should().Be(userId);
    }

    [Fact]
    public void LinkUser_SameUserAgain_SucceedsIdempotently()
    {
        var client = Client.Create(Contact());
        var userId = Guid.CreateVersion7();
        client.LinkUser(userId);

        var result = client.LinkUser(userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void LinkUser_DifferentUser_FailsWithAlreadyLinkedError()
    {
        var client = Client.Create(Contact());
        client.LinkUser(Guid.CreateVersion7());

        var result = client.LinkUser(Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Client.AlreadyLinkedToDifferentUser);
    }

    [Fact]
    public void LinkUser_EmptyGuid_FailsWithValidationError()
    {
        var client = Client.Create(Contact());

        var result = client.LinkUser(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}