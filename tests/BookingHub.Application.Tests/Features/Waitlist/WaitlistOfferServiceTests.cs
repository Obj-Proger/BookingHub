using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class WaitlistOfferServiceTests
{
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();
    private static readonly Guid ServiceId = Guid.CreateVersion7();
    private static readonly TimeSlot FreedSlot = TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value;

    private WaitlistOfferService CreateSut() => new(_waitlistEntryRepository, _dbContext, _unitOfWork);

    private static WaitlistEntry CreateWaitingEntry() => WaitlistEntry.Create(
        OrganizationId, LocationId, null, ServiceId,
        ClientContact.Create(PhoneNumber.Create("+14155552671").Value), FreedSlot, DateTime.UtcNow).Value;

    private void SetUpOrganization(TimeSpan offerWindow)
    {
        var organization = Organization.Create("Name", "slug").Value;
        organization.UpdateWaitlistOfferWindow(offerWindow);
        _dbContext.Organizations.Returns(new[] { organization }.AsQueryable());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_NoCandidates_DoesNothing()
    {
        _waitlistEntryRepository.GetWaitingCandidatesAsync(OrganizationId, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([]);
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_OneCandidate_OffersAndSaves()
    {
        var entry = CreateWaitingEntry();
        _waitlistEntryRepository.GetWaitingCandidatesAsync(OrganizationId, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([entry]);
        SetUpOrganization(TimeSpan.FromHours(2));
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        entry.Status.Should().Be(WaitlistEntryStatus.Offered);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_FirstCandidateCannotBeOffered_FallsThroughToNext()
    {
        var firstEntry = CreateWaitingEntry();
        firstEntry.Cancel(DateTime.UtcNow); // no longer Waiting — Offer() will fail
        var secondEntry = CreateWaitingEntry();
        _waitlistEntryRepository.GetWaitingCandidatesAsync(OrganizationId, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([firstEntry, secondEntry]);
        SetUpOrganization(TimeSpan.FromHours(2));
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        secondEntry.Status.Should().Be(WaitlistEntryStatus.Offered);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_AllCandidatesFail_DoesNotSave()
    {
        var entry = CreateWaitingEntry();
        entry.Cancel(DateTime.UtcNow);
        _waitlistEntryRepository.GetWaitingCandidatesAsync(OrganizationId, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([entry]);
        SetUpOrganization(TimeSpan.FromHours(2));
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(OrganizationId, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}