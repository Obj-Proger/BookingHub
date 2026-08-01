using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Locations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Locations.Queries.GetLocation;

internal sealed class GetLocationQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationQuery, LocationResponse>
{
    public async Task<Result<LocationResponse>> Handle(GetLocationQuery query, CancellationToken cancellationToken)
    {
        var response = await dbContext.Locations
            .Where(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId)
            .Select(l => new LocationResponse(l.Id, l.Name, l.Address.Value, l.TimeZone))
            .FirstOrDefaultAsync(cancellationToken);

        return response is not null ? response : Result.Failure<LocationResponse>(ApplicationErrors.Location.NotFound);
    }
}