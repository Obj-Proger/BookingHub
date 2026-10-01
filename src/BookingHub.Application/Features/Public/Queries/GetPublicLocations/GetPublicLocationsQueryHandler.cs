using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Public.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Public.Queries.GetPublicLocations;

internal sealed class GetPublicLocationsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPublicLocationsQuery, IReadOnlyList<PublicLocationResponse>>
{
    public async Task<Result<IReadOnlyList<PublicLocationResponse>>> Handle(
        GetPublicLocationsQuery query, CancellationToken cancellationToken)
    {
        var locations = await dbContext.Locations
            .Where(l => l.OrganizationId == query.OrganizationId)
            .OrderBy(l => l.Name)
            .Select(l => new PublicLocationResponse(l.Id, l.Name, l.Address.Value, l.TimeZone))
            .ToListAsync(cancellationToken);

        return locations;
    }
}