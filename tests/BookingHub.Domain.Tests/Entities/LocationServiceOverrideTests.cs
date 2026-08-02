namespace BookingHub.Domain.Tests.Entities;

public class LocationServiceOverrideTests
{
    private static readonly Money ValidPrice = Money.Create(45m, "USD").Value;

    [Fact]
    public void Create_ValidIds_Succeeds()
    {
        var result = LocationServiceOverride.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), ValidPrice);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverridePrice.Should().Be(ValidPrice);
    }

    [Fact]
    public void Create_EmptyLocationId_FailsWithValidationError()
    {
        var result = LocationServiceOverride.Create(Guid.Empty, Guid.CreateVersion7(), ValidPrice);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_EmptyServiceId_FailsWithValidationError()
    {
        var result = LocationServiceOverride.Create(Guid.CreateVersion7(), Guid.Empty, ValidPrice);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void UpdatePrice_UpdatesOverridePrice()
    {
        var @override = LocationServiceOverride.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), ValidPrice).Value;
        var newPrice = Money.Create(60m, "USD").Value;

        @override.UpdatePrice(newPrice);

        @override.OverridePrice.Should().Be(newPrice);
    }
}