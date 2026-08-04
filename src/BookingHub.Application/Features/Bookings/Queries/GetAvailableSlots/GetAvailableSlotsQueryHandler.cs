using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Application.Features.Bookings.Queries.GetAvailableSlots;
using BookingHub.Domain.Services;

internal sealed class GetAvailableSlotsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<IReadOnlyList<AvailableSlotResponse>>> Handle(
        GetAvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var contextResult = await AvailabilityContextLoader.LoadAsync(
            dbContext, query.OrganizationId, query.LocationId, query.EmployeeId, query.ServiceId, query.Date, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<IReadOnlyList<AvailableSlotResponse>>(contextResult.Error);

        var context = contextResult.Value;
        if (context.Assignment is null)
            return new List<AvailableSlotResponse>();

        var availableSlots = AvailabilityCalculator.CalculateAvailableSlots(
            context.Location.WorkingHours, context.RecurringSchedule, context.ExceptionForDate, context.OccupiedWindows,
            context.Service.Duration, context.Service.BufferBefore, context.Service.BufferAfter,
            query.Date, context.TimeZone, SlotGranularity);

        return availableSlots.Select(s => new AvailableSlotResponse(s.StartUtc, s.EndUtc)).ToList();
    }
}