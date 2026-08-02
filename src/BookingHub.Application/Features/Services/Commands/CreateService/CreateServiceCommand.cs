using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Services.DTOs;

namespace BookingHub.Application.Features.Services.Commands.CreateService;

public sealed record CreateServiceCommand(
    Guid OrganizationId, string? Name, TimeSpan Duration, decimal BasePriceAmount, string? BasePriceCurrency,
    TimeSpan BufferBefore, TimeSpan BufferAfter, string? Color)
    : ICommand<ServiceCreatedResponse>, IRequireOrganizationManagement;