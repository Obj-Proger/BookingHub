namespace BookingHub.Domain.Tests.ValueObjects;

public class ClientContactTests
{
    private static readonly PhoneNumber SamplePhone = PhoneNumber.Create("+14155552671").Value;

    [Fact]
    public void Create_WithNameAndEmail_SetsAllProperties()
    {
        var email = Email.Create("user@example.com").Value;

        var contact = ClientContact.Create(SamplePhone, "  Jane Doe  ", email);

        contact.Phone.Should().Be(SamplePhone);
        contact.Name.Should().Be("Jane Doe");
        contact.Email.Should().Be(email);
    }

    [Fact]
    public void Create_WithoutNameOrEmail_LeavesThemNull()
    {
        var contact = ClientContact.Create(SamplePhone);

        contact.Name.Should().BeNull();
        contact.Email.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhitespaceOnlyName_IsNormalizedToNull(string name)
    {
        var contact = ClientContact.Create(SamplePhone, name);

        contact.Name.Should().BeNull();
    }

    [Fact]
    public void Equals_SamePhoneNameAndEmail_ReturnsTrue()
    {
        var first = ClientContact.Create(SamplePhone, "Jane Doe");
        var second = ClientContact.Create(SamplePhone, "Jane Doe");

        first.Should().Be(second);
    }
}