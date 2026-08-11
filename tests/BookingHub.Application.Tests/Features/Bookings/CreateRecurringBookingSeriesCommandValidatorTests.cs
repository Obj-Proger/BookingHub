using BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;

namespace BookingHub.Application.Tests.Features.Bookings;

public class CreateRecurringBookingSeriesCommandValidatorTests
{
    private readonly CreateRecurringBookingSeriesCommandValidator _validator = new();

    private static CreateRecurringBookingSeriesCommand ValidCommand(int intervalWeeks = 2, int occurrenceCount = 6) => new(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
        DateTime.UtcNow.AddDays(1), intervalWeeks, occurrenceCount, "+14155552671", "Jane Doe", null);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_IntervalWeeksOutOfRange_HasError(int intervalWeeks)
    {
        var result = _validator.Validate(ValidCommand(intervalWeeks: intervalWeeks));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(53)]
    public void Validate_OccurrenceCountOutOfRange_HasError(int occurrenceCount)
    {
        var result = _validator.Validate(ValidCommand(occurrenceCount: occurrenceCount));

        result.IsValid.Should().BeFalse();
    }
}