using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Organizations.Queries.GetOrganization;

internal sealed class GetOrganizationQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrganizationQuery, OrganizationResponse>
{
    public async Task<Result<OrganizationResponse>> Handle(GetOrganizationQuery query, CancellationToken cancellationToken)
    {
        var response = await dbContext.Organizations
            .Where(o => o.Id == query.OrganizationId)
            .Select(o => new OrganizationResponse(
                o.Id, o.Name, o.Slug, o.CancellationDeadlineHours,
                o.PendingConfirmationWindow, o.AutoCompleteWindow, o.WaitlistOfferWindow))
            .FirstOrDefaultAsync(cancellationToken);

        return response is not null
            ? response
            : Result.Failure<OrganizationResponse>(ApplicationErrors.Organization.NotFound);
    }
}