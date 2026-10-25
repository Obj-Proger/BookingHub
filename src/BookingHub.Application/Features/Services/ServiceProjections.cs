using System.Linq.Expressions;
using BookingHub.Application.Features.Services.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Services;

internal static class ServiceProjections
{
    public static readonly Expression<Func<Service, ServiceResponse>> ToResponse = s => new ServiceResponse(
        s.Id, s.Name, s.Duration, s.BasePrice.Amount, s.BasePrice.Currency, s.BufferBefore, s.BufferAfter, s.Color);
}