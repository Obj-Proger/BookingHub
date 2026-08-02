using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Services.Commands.UpdateLocationServiceOverridePrice;

public sealed record UpdateLocationServiceOverridePriceCommand(
    Guid OrganizationId, Guid LocationId, Guid OverrideId, decimal NewAmount, string? NewCurrency)
    : ICommand, IRequireLocationManagement;