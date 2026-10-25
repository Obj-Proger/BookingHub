using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Services.Queries.GetServices;

internal sealed class GetServicesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceResponse>>
{
    public async Task<Result<IReadOnlyList<ServiceResponse>>> Handle(GetServicesQuery query, CancellationToken cancellationToken)
    {
        var services = await dbContext.Services
            .Where(s => s.OrganizationId == query.OrganizationId)
            .OrderBy(s => s.Name)
            .Select(ServiceProjections.ToResponse)
            .ToListAsync(cancellationToken);

        return services;
    }
}