using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IOrganizationMemberRepository
{
    void Add(OrganizationMember member);
}