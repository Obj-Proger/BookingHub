namespace BookingHub.Domain.Tests.Entities;

public class ServiceTests
{
    private static readonly Guid ValidOrganizationId = Guid.CreateVersion7();
    private static readonly Money ValidPrice = Money.Create(50m, "USD").Value;
    private static readonly TimeSpan ValidDuration = TimeSpan.FromMinutes(30);

    private static Result<Service> CreateValidService(string? color = "#FF5733") =>
        Service.Create(ValidOrganizationId, "Haircut", ValidDuration, ValidPrice, TimeSpan.Zero, TimeSpan.Zero, color);

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var result = CreateValidService();

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Haircut");
        result.Value.Duration.Should().Be(ValidDuration);
        result.Value.BasePrice.Should().Be(ValidPrice);
        result.Value.Color.Should().Be("#FF5733");
    }

    [Fact]
    public void Create_LowercaseColor_IsNormalizedToUppercase()
    {
        var result = CreateValidService("#ff5733");

        result.Value.Color.Should().Be("#FF5733");
    }

    [Fact]
    public void Create_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = Service.Create(Guid.Empty, "Haircut", ValidDuration, ValidPrice, TimeSpan.Zero, TimeSpan.Zero, "#FF5733");

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_FailsWithNameEmptyError(string? name)
    {
        var result = Service.Create(ValidOrganizationId, name, ValidDuration, ValidPrice, TimeSpan.Zero, TimeSpan.Zero, "#FF5733");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.NameEmpty);
    }

    [Theory]
    [MemberData(nameof(NonPositiveDurations))]
    public void Create_DurationNotPositive_FailsWithDurationNotPositiveError(TimeSpan duration)
    {
        var result = Service.Create(ValidOrganizationId, "Haircut", duration, ValidPrice, TimeSpan.Zero, TimeSpan.Zero, "#FF5733");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.DurationNotPositive);
    }

    public static TheoryData<TimeSpan> NonPositiveDurations() => new() { TimeSpan.Zero, TimeSpan.FromMinutes(-30) };

    [Fact]
    public void Create_NegativeBufferBefore_FailsWithNegativeBufferError()
    {
        var result = Service.Create(
            ValidOrganizationId, "Haircut", ValidDuration, ValidPrice, TimeSpan.FromMinutes(-5), TimeSpan.Zero, "#FF5733");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.NegativeBuffer);
    }

    [Fact]
    public void Create_NegativeBufferAfter_FailsWithNegativeBufferError()
    {
        var result = Service.Create(
            ValidOrganizationId, "Haircut", ValidDuration, ValidPrice, TimeSpan.Zero, TimeSpan.FromMinutes(-5), "#FF5733");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.NegativeBuffer);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("FF5733")]     // missing '#'
    [InlineData("#FFF")]       // too few digits
    [InlineData("#FF57333")]   // too many digits
    [InlineData("#GG5733")]    // invalid hex digits
    public void Create_InvalidColor_FailsWithInvalidColorError(string? color)
    {
        var result = CreateValidService(color);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.InvalidColor);
    }

    [Fact]
    public void Rename_ValidNewName_UpdatesName()
    {
        var service = CreateValidService().Value;

        var result = service.Rename("Deluxe Haircut");

        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Deluxe Haircut");
    }

    [Fact]
    public void UpdatePricing_UpdatesBasePrice()
    {
        var service = CreateValidService().Value;
        var newPrice = Money.Create(75m, "USD").Value;

        service.UpdatePricing(newPrice);

        service.BasePrice.Should().Be(newPrice);
    }

    [Fact]
    public void UpdateDuration_ValidDuration_Succeeds()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateDuration(TimeSpan.FromMinutes(45));

        result.IsSuccess.Should().BeTrue();
        service.Duration.Should().Be(TimeSpan.FromMinutes(45));
    }

    [Fact]
    public void UpdateDuration_NotPositive_FailsWithDurationNotPositiveError()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateDuration(TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.DurationNotPositive);
    }

    [Fact]
    public void UpdateBuffers_ValidBuffers_Succeeds()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateBuffers(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));

        result.IsSuccess.Should().BeTrue();
        service.BufferBefore.Should().Be(TimeSpan.FromMinutes(5));
        service.BufferAfter.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void UpdateBuffers_NegativeBuffer_FailsWithNegativeBufferError()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateBuffers(TimeSpan.FromMinutes(-1), TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.NegativeBuffer);
    }

    [Fact]
    public void UpdateColor_ValidColor_Succeeds()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateColor("#00FF00");

        result.IsSuccess.Should().BeTrue();
        service.Color.Should().Be("#00FF00");
    }

    [Fact]
    public void UpdateColor_InvalidColor_FailsWithInvalidColorError()
    {
        var service = CreateValidService().Value;

        var result = service.UpdateColor("not-a-color");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Service.InvalidColor);
    }
}