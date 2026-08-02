using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Services.Commands.CreateLocationServiceOverride;

internal sealed class CreateLocationServiceOverrideCommandHandler(
    ILocationRepository locationRepository,
    IServiceRepository serviceRepository,
    ILocationServiceOverrideRepository overrideRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateLocationServiceOverrideCommand, LocationServiceOverrideCreatedResponse>
{
    public async Task<Result<LocationServiceOverrideCreatedResponse>> Handle(
        CreateLocationServiceOverrideCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<LocationServiceOverrideCreatedResponse>(ApplicationErrors.Location.NotFound);

        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<LocationServiceOverrideCreatedResponse>(ApplicationErrors.Service.NotFound);

        var priceResult = Money.Create(command.OverridePriceAmount, command.OverridePriceCurrency);
        if (priceResult.IsFailure)
            return Result.Failure<LocationServiceOverrideCreatedResponse>(priceResult.Error);

        if (priceResult.Value.Currency != service.BasePrice.Currency)
            return Result.Failure<LocationServiceOverrideCreatedResponse>(ApplicationErrors.LocationServiceOverride.CurrencyMismatch);

        if (await overrideRepository.ExistsForServiceAsync(location.Id, service.Id, cancellationToken))
            return Result.Failure<LocationServiceOverrideCreatedResponse>(ApplicationErrors.LocationServiceOverride.AlreadyExists);

        var overrideResult = LocationServiceOverride.Create(location.Id, service.Id, priceResult.Value);
        if (overrideResult.IsFailure)
            return Result.Failure<LocationServiceOverrideCreatedResponse>(overrideResult.Error);

        overrideRepository.Add(overrideResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LocationServiceOverrideCreatedResponse(
            overrideResult.Value.Id, location.Id, service.Id, priceResult.Value.Amount, priceResult.Value.Currency);
    }
}