using FluentValidation;

namespace BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;

/// <summary>
/// Interval/count are batch-scheduling policy, not a rule any single Domain aggregate owns —
/// the first real production use of FluentValidation in this project, exactly the case it was
/// reserved for back when we decided Domain-owned fields never get a duplicate check here.
/// </summary>
public sealed class CreateRecurringBookingSeriesCommandValidator : AbstractValidator<CreateRecurringBookingSeriesCommand>
{
    public CreateRecurringBookingSeriesCommandValidator()
    {
        RuleFor(c => c.IntervalWeeks).InclusiveBetween(1, 12);
        RuleFor(c => c.OccurrenceCount).InclusiveBetween(2, 52);
    }
}