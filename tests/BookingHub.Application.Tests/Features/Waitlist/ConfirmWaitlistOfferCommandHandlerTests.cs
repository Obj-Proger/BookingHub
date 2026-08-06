using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class ConfirmWaitlistOfferCommandHandlerTests
{
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ConfirmWaitlistOfferCommandHandler CreateSut() =>
        new(_waitlistEntryRepository, _dbContext, _clientRepository, _bookingRepository, _unitOfWork);

    private static WaitlistEntry CreateOfferedEntry()
    {
        var entry = WaitlistEntry.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;
        entry.Offer(
            Guid.CreateVersion7(), TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value,
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow);
        return entry;
    }

    [Fact]
    public async Task Handle_EntryNotFound_FailsWithNotFoundError()
    {
        _waitlistEntryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WaitlistEntry?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmWaitlistOfferCommand(Guid.CreateVersion7(), "any-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.WaitlistEntry.NotFound);
    }

    [Fact]
    public async Task Handle_WrongToken_FailsWithoutCheckingStatus()
    {
        var entry = CreateOfferedEntry();
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmWaitlistOfferCommand(entry.Id, "wrong-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.WaitlistEntry.InvalidManagementToken);
    }

    [Fact]
    public async Task Handle_EntryNotOffered_FailsWithDomainCannotConvertError()
    {
        var entry = WaitlistEntry.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(),
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value, DateTime.UtcNow).Value;
        _waitlistEntryRepository.GetByIdAsync(entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var sut = CreateSut();

        var result = await sut.Handle(new ConfirmWaitlistOfferCommand(entry.Id, entry.ManagementToken.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotConvert);
    }
}