using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Locations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Locations.Queries.GetLocations;

internal sealed class GetLocationsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationsQuery, IReadOnlyList<LocationResponse>>
{
    public async Task<Result<IReadOnlyList<LocationResponse>>> Handle(GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var locations = await dbContext.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == query.OrganizationId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        return locations.Select(LocationResponseMapper.ToResponse).ToList();
    }
}