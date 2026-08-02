using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Locations.Commands.UpdateLocationAddress;

internal sealed class UpdateLocationAddressCommandHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateLocationAddressCommand>
{
    public async Task<Result> Handle(UpdateLocationAddressCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure(ApplicationErrors.Location.NotFound);

        var addressResult = Address.Create(command.NewAddress);
        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);

        location.Relocate(addressResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}