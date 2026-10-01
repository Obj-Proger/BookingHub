using BookingHub.Application.Common;
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
        var services = await dbContext.Services
            .Where(s => s.OrganizationId == query.OrganizationId)
            .GroupJoin(
                dbContext.LocationServiceOverrides.Where(o => o.LocationId == query.LocationId),
                s => s.Id, o => o.ServiceId, (s, overrides) => new { Service = s, Overrides = overrides })
            .SelectMany(x => x.Overrides.DefaultIfEmpty(), (x, overrideRow) => new PublicServiceResponse(
                x.Service.Id, x.Service.Name, x.Service.Duration,
                overrideRow != null ? overrideRow.OverridePrice.Amount : x.Service.BasePrice.Amount,
                overrideRow != null ? overrideRow.OverridePrice.Currency : x.Service.BasePrice.Currency))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return services;
    }
}