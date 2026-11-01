using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Queries.GetAvailableSlots;

internal sealed class GetAvailableSlotsQueryHandler(SlotAvailabilityService slotAvailability)
    : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    public async Task<Result<IReadOnlyList<AvailableSlotResponse>>> Handle(
        GetAvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var slotsResult = await slotAvailability.GetAvailableSlotsAsync(
            query.OrganizationId, query.LocationId, query.EmployeeId, query.ServiceId, query.Date, cancellationToken);
        if (slotsResult.IsFailure)
            return Result.Failure<IReadOnlyList<AvailableSlotResponse>>(slotsResult.Error);

        return slotsResult.Value.Select(s => new AvailableSlotResponse(s.StartUtc, s.EndUtc)).ToList();
    }
}