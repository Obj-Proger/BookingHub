using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Services.Queries.GetService;

internal sealed class GetServiceQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetServiceQuery, ServiceResponse>
{
    public async Task<Result<ServiceResponse>> Handle(GetServiceQuery query, CancellationToken cancellationToken)
    {
        var response = await dbContext.Services
            .Where(s => s.Id == query.ServiceId && s.OrganizationId == query.OrganizationId)
            .Select(s => new ServiceResponse(
                s.Id, s.Name, s.Duration, s.BasePrice.Amount, s.BasePrice.Currency, s.BufferBefore, s.BufferAfter, s.Color))
            .FirstOrDefaultAsync(cancellationToken);

        return response is not null ? response : Result.Failure<ServiceResponse>(ApplicationErrors.Service.NotFound);
    }
}