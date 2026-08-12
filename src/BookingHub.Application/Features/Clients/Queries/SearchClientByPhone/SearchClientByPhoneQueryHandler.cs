using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Clients.DTOs;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Clients.Queries.SearchClientByPhone;

internal sealed class SearchClientByPhoneQueryHandler(IClientRepository clientRepository, IApplicationDbContext dbContext)
    : IQueryHandler<SearchClientByPhoneQuery, ClientSearchResultResponse>
{
    public async Task<Result<ClientSearchResultResponse>> Handle(SearchClientByPhoneQuery query, CancellationToken cancellationToken)
    {
        var phoneResult = PhoneNumber.Create(query.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<ClientSearchResultResponse>(phoneResult.Error);

        var client = await clientRepository.GetByPhoneAsync(phoneResult.Value, cancellationToken);
        if (client is null)
            return Result.Failure<ClientSearchResultResponse>(ApplicationErrors.Client.NotFound);

        var hasVisitedThisOrganization = await dbContext.Bookings
            .AnyAsync(b => b.OrganizationId == query.OrganizationId && b.ClientId == client.Id, cancellationToken);

        return hasVisitedThisOrganization
            ? new ClientSearchResultResponse(client.Id, client.Phone.Value, client.Name)
            : Result.Failure<ClientSearchResultResponse>(ApplicationErrors.Client.NotFound);
    }
}