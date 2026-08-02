using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Services.Commands.UpdateLocationServiceOverridePrice;

internal sealed class UpdateLocationServiceOverridePriceCommandHandler(
    ILocationServiceOverrideRepository overrideRepository, IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateLocationServiceOverridePriceCommand>
{
    public async Task<Result> Handle(UpdateLocationServiceOverridePriceCommand command, CancellationToken cancellationToken)
    {
        var @override = await overrideRepository.GetByIdAsync(command.LocationId, command.OverrideId, cancellationToken);
        if (@override is null)
            return Result.Failure(ApplicationErrors.LocationServiceOverride.NotFound);

        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, @override.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var priceResult = Money.Create(command.NewAmount, command.NewCurrency);
        if (priceResult.IsFailure)
            return Result.Failure(priceResult.Error);

        if (priceResult.Value.Currency != service.BasePrice.Currency)
            return Result.Failure(ApplicationErrors.LocationServiceOverride.CurrencyMismatch);

        @override.UpdatePrice(priceResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}