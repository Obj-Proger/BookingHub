using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Clients.DTOs;
using BookingHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Clients.Queries.GetClientProfile;

internal sealed class GetClientProfileQueryHandler(
    IClientRepository clientRepository,
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IOrganizationMemberRepository organizationMemberRepository)
    : IQueryHandler<GetClientProfileQuery, ClientProfileResponse>
{
    public async Task<Result<ClientProfileResponse>> Handle(GetClientProfileQuery query, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientProfileResponse>(ApplicationErrors.Client.NotFound);

        // AuthorizationBehavior already confirmed membership — re-fetched here (not passed
        // through the pipeline) because the handler needs the caller's Role/EmployeeId, not
        // just the yes/no membership answer the behavior itself checks.
        var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(query.OrganizationId, currentUser.UserId, cancellationToken);
        if (caller is null)
            return Result.Failure<ClientProfileResponse>(ApplicationErrors.Authorization.NotAMember);

        var bookingsQuery = dbContext.Bookings
            .Where(b => b.OrganizationId == query.OrganizationId && b.ClientId == query.ClientId);

        // Vision Document role matrix: an Employee sees only their own visit history with this
        // client; Owner/Administrator/LocationManager see the client's full history at this org.
        if (caller.Role == OrganizationRole.Employee)
            bookingsQuery = bookingsQuery.Where(b => b.EmployeeId == caller.EmployeeId);

        var visits = await (
            from b in bookingsQuery
            join s in dbContext.Services on b.ServiceId equals s.Id
            join e in dbContext.Employees on b.EmployeeId equals e.Id
            orderby b.TimeSlot.StartUtc descending
            select new ClientVisitResponse(b.Id, b.TimeSlot.StartUtc, s.Name, e.FullName, b.Status, b.Price.Amount, b.Price.Currency))
            .ToListAsync(cancellationToken);

        var completedVisits = visits.Where(v => v.Status == BookingStatus.Completed).ToList();

        var totalRevenue = completedVisits
            .GroupBy(v => v.PriceCurrency)
            .Select(g => new RevenueByCurrency(g.Key, g.Sum(v => v.PriceAmount)))
            .ToList();

        var averageCheck = completedVisits
            .GroupBy(v => v.PriceCurrency)
            .Select(g => new RevenueByCurrency(g.Key, Math.Round(g.Average(v => v.PriceAmount), 2)))
            .ToList();

        return new ClientProfileResponse(
            client.Id, client.Phone.Value, client.Name, client.Email?.Value,
            visits.Count, completedVisits.Count, visits.Count(v => v.Status == BookingStatus.NoShow),
            totalRevenue, averageCheck, visits);
    }
}