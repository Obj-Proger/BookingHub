using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Public.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Public.Queries.GetPublicServices;

internal sealed class GetPublicServicesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPublicServicesQuery, IReadOnlyList<PublicServiceResponse>>
{
    public async Task<Result<IReadOnlyList<PublicServiceResponse>>> Handle(
        GetPublicServicesQuery query, CancellationToken cancellationToken)
    {
        // Two flat queries merged in memory: EF Core could not translate the left join with the ordering
        // over the projected record, and an organization has only a handful of services.
        var services = await dbContext.Services
            .Where(s => s.OrganizationId == query.OrganizationId)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Duration, s.BasePrice.Amount, s.BasePrice.Currency })
            .ToListAsync(cancellationToken);

        var overrides = await dbContext.LocationServiceOverrides
            .Where(o => o.LocationId == query.LocationId)
            .Select(o => new { o.ServiceId, o.OverridePrice.Amount, o.OverridePrice.Currency })
            .ToDictionaryAsync(o => o.ServiceId, cancellationToken);

        return services
            .Select(s => overrides.TryGetValue(s.Id, out var price)
                ? new PublicServiceResponse(s.Id, s.Name, s.Duration, price.Amount, price.Currency)
                : new PublicServiceResponse(s.Id, s.Name, s.Duration, s.Amount, s.Currency))
            .ToList();
    }
}
