using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Services.Queries.GetLocationServiceOverrides;

internal sealed class GetLocationServiceOverridesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationServiceOverridesQuery, IReadOnlyList<LocationServiceOverrideResponse>>
{
    public async Task<Result<IReadOnlyList<LocationServiceOverrideResponse>>> Handle(
        GetLocationServiceOverridesQuery query, CancellationToken cancellationToken)
    {
        var locationExists = await dbContext.Locations
            .AnyAsync(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId, cancellationToken);

        if (!locationExists)
            return Result.Failure<IReadOnlyList<LocationServiceOverrideResponse>>(ApplicationErrors.Location.NotFound);

        var overrides = await dbContext.LocationServiceOverrides
            .Where(o => o.LocationId == query.LocationId)
            .Select(o => new LocationServiceOverrideResponse(o.Id, o.ServiceId, o.OverridePrice.Amount, o.OverridePrice.Currency))
            .ToListAsync(cancellationToken);

        return overrides;
    }
}