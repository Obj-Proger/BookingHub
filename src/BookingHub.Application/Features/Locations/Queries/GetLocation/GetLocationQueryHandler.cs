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
        var location = await dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId, cancellationToken);

        return location is not null
            ? LocationResponseMapper.ToResponse(location)
            : Result.Failure<LocationResponse>(ApplicationErrors.Location.NotFound);
    }
}