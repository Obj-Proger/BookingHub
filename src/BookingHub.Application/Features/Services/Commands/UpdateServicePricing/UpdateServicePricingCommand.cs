using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.UpdateServicePricing;

public sealed record UpdateServicePricingCommand(Guid OrganizationId, Guid ServiceId, decimal NewAmount, string? NewCurrency)
    : ICommand, IRequireOrganizationManagement;